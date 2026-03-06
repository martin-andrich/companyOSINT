# companyOSINT

.NET 10 Solution mit einer Datenbank von Firmen und deren Metadaten.

## Projekt auf dem vServer starten

### Voraussetzungen

- Docker & Docker Compose
- Git
- Nginx (als Reverse Proxy)
- PostgreSQL (nativ auf dem Server installiert)
- Die Datei `handelsregister.db` (wird für den Import benötigt)

### 1. PostgreSQL einrichten

PostgreSQL muss nativ auf dem Server installiert sein. Die Docker-Container greifen über `host.docker.internal` auf den Host-Postgres zu.

#### Datenbank und Benutzer anlegen

```bash
sudo -u postgres psql
```

```sql
CREATE DATABASE "companyOSINT";
-- Optional: eigenen Benutzer anlegen
CREATE USER companyosint WITH PASSWORD 'sicheres-passwort';
GRANT ALL PRIVILEGES ON DATABASE "companyOSINT" TO companyosint;
\c "companyOSINT"
GRANT ALL ON SCHEMA public TO companyosint;
```

#### `postgresql.conf` anpassen

PostgreSQL hört standardmäßig nur auf `localhost`. Damit Docker-Container zugreifen können, muss die Docker-Bridge-IP ergänzt werden.

```bash
sudo nano /etc/postgresql/*/main/postgresql.conf
```

Zeile `listen_addresses` anpassen:

```
listen_addresses = 'localhost,172.17.0.1'
```

`172.17.0.1` ist die Standard-Gateway-IP des Docker-Bridge-Netzwerks. Alternativ `'*'` für alle Interfaces.

#### `pg_hba.conf` anpassen

Docker-Containern den Zugriff auf die Datenbank erlauben:

```bash
sudo nano /etc/postgresql/*/main/pg_hba.conf
```

Folgende Zeile am Ende hinzufügen:

```
# Docker-Container
host    all    all    172.16.0.0/12    scram-sha-256
```

Das Subnetz `172.16.0.0/12` deckt alle möglichen Docker-Bridge-Netze ab (`172.16.x.x` – `172.31.x.x`).

#### PostgreSQL neu starten

```bash
sudo systemctl restart postgresql
```

#### Verbindung testen

```bash
# Vom Host aus:
psql -h 172.17.0.1 -U postgres -d companyOSINT

# Aus einem Docker-Container:
docker run --rm --add-host=host.docker.internal:host-gateway postgres:latest \
  psql -h host.docker.internal -U postgres -d companyOSINT -c "SELECT 1"
```

### 2. Repository klonen & `.env` anlegen

```bash
git clone <repo-url> companyOSINT
cd companyOSINT
cp .env.example .env
# Dann .env bearbeiten und sichere Werte eintragen:
#   CONNECTION_STRING=Host=host.docker.internal;Database=companyOSINT;Username=postgres;Password=...
#   API_KEY=...
```

### 3. Starten

```bash
docker compose -f compose.prod.yaml up -d --build --remove-orphans
docker image prune -f
```

Die API ist danach auf `127.0.0.1:8085` erreichbar (nur lokal, nicht von außen).

## Daten importieren

Der Import liest die SQLite-Datei `handelsregister.db` und schreibt die Daten nach PostgreSQL. Die Datei muss im Projektverzeichnis liegen.

```bash
# handelsregister.db ins Projektverzeichnis kopieren, dann:
docker compose -f compose.prod.yaml --profile import run --rm --build --remove-orphans companyosint-import
docker image prune -f
```

Der Import-Container startet, verarbeitet die Daten und beendet sich automatisch.

## Worker starten

Der Worker (Company Enrichment) läuft nicht standardmäßig mit und muss bei Bedarf separat gestartet werden:

```bash
docker compose -f compose.prod.yaml --profile worker up -d --build companyosint-worker
docker image prune -f
```

Stoppen:

```bash
docker compose -f compose.prod.yaml --profile worker stop companyosint-worker
```

## Projekt aktualisieren

```bash
cd companyOSINT
git pull
docker compose -f compose.prod.yaml up -d --build --remove-orphans
docker image prune -f
```

Docker baut nur die Container neu, bei denen sich etwas geändert hat. Die PostgreSQL-Daten liegen auf dem Host und sind davon nicht betroffen.

## API-Authentifizierung

Alle `/api/*`-Endpoints sind mit einem API Key geschützt. Der Key muss als `X-API-Key`-Header mitgesendet werden.

### Konfiguration

Beide Services (Web + Worker) lesen den Key aus der Umgebungsvariable `API_KEY` (bzw. `ApiKey`). In der `.env`-Datei einen sicheren Key eintragen:

```bash
# Sicheren Key generieren:
openssl rand -hex 32
```

Docker Compose übergibt den Key automatisch an beide Services.

### Lokale Entwicklung (ohne Docker)

Für `dotnet run --project companyOSINT.Web` liegt ein Dev-Key in `appsettings.Development.json` (`ApiKey`). Der Worker liest den Key aus der `.env`-Datei im Projektverzeichnis.

### Endpoints testen

```bash
# Ohne Key → 401 Unauthorized
curl -i https://deine-domain.de/api/companies

# Mit Key → 200 OK
curl -i -H "X-API-Key: DEIN_KEY" https://deine-domain.de/api/companies
```

Der OpenAPI-Endpoint (`/openapi/v1.json`) ist nicht geschützt und nur im Development-Modus aktiv.

## EF Core Migrationen

Das Datenbankschema wird über EF Core Migrationen verwaltet. Der `ApplicationDbContext` liegt in `companyOSINT.Shared`, die Migrationen in `companyOSINT.Web`.

### Neue Migration erstellen

```bash
dotnet ef migrations add <MigrationName> --project companyOSINT.Web --startup-project companyOSINT.Web
```

### Migrationen auf die Datenbank anwenden

```bash
dotnet ef database update --project companyOSINT.Web --startup-project companyOSINT.Web
```

## PostgreSQL Backup & Restore

### Backup erstellen

```bash
pg_dump -U postgres companyOSINT > backup_$(date +%Y%m%d_%H%M%S).sql
```

### Backup wiederherstellen

```bash
# Bestehende Datenbank leeren und Backup einspielen:
psql -U postgres -d companyOSINT -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"
psql -U postgres -d companyOSINT < backup_20260220_143000.sql
```

## Logs & Stoppen

| Aktion                      | Befehl                                                      |
|-----------------------------|-------------------------------------------------------------|
| Alle Logs ansehen           | `docker compose -f compose.prod.yaml logs -f`               |
| Nur Web-Logs                | `docker compose -f compose.prod.yaml logs -f companyosint-web` |
| Projekt stoppen             | `docker compose -f compose.prod.yaml down`                  |
| Neu bauen & starten         | `docker compose -f compose.prod.yaml up -d --build`         |
| Status der Container        | `docker compose -f compose.prod.yaml ps`                    |
| Was belegt wieviel Speicher | `docker system df`                                          |
| Speicher freigeben          | `docker system prune -a --volumes -f`                       |


## Nginx Reverse Proxy

Beispielkonfiguration unter `/etc/nginx/sites-available/findACompany`:

```nginx
server {
    listen 443 ssl;
    server_name deine-domain.de;

    ssl_certificate     /etc/letsencrypt/live/deine-domain.de/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/deine-domain.de/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:8085;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Aktivieren und nginx neu laden:

```bash
sudo ln -s /etc/nginx/sites-available/findACompany /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```
