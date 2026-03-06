# companyOSINT

.NET 10 Solution mit einer Datenbank von Firmen und deren Metadaten.

## Projekt auf dem vServer starten

### Voraussetzungen

- Docker & Docker Compose
- Git
- Nginx (als Reverse Proxy)
- Die Datei `handelsregister.db` (wird für den Import benötigt)

### 1. Repository klonen & `.env` anlegen

```bash
git clone <repo-url> companyOSINT
cd companyOSINT
cp .env.example .env
# Dann .env bearbeiten und sichere Werte eintragen:
#   POSTGRES_PASSWORD=...
#   API_KEY=...
```

### 2. Starten

```bash
docker compose -f compose.prod.yaml up -d --build --remove-orphans
docker image prune -f
```

Die API ist danach auf `127.0.0.1:8085` erreichbar (nur lokal, nicht von außen).

## Daten importieren

Der Import liest die SQLite-Datei `handelsregister.db` und schreibt die Daten nach PostgreSQL. Die Datei muss im Projektverzeichnis liegen.

```bash
# handelsregister.db ins Projektverzeichnis kopieren, dann:
docker compose -f compose.prod.yaml --profile import run --rm --build --remove-orphans findACompany-import
docker image prune -f
```

Der Import-Container startet, verarbeitet die Daten und beendet sich automatisch.

## Worker starten

Der Worker (Company Enrichment) läuft nicht standardmäßig mit und muss bei Bedarf separat gestartet werden:

```bash
docker compose -f compose.prod.yaml --profile worker up -d --build findACompany-worker
docker image prune -f
```

Stoppen:

```bash
docker compose -f compose.prod.yaml --profile worker stop findACompany-worker
```

## Projekt aktualisieren

```bash
cd companyOSINT
git pull
docker compose -f compose.prod.yaml up -d --build --remove-orphans
docker image prune -f
```

Docker baut nur die Container neu, bei denen sich etwas geändert hat. Die PostgreSQL-Daten bleiben im Volume erhalten.

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
docker compose -f compose.prod.yaml exec postgres pg_dump -U postgres companyOSINT > backup_$(date +%Y%m%d_%H%M%S).sql
```

Die Datei (z.B. `backup_20260220_143000.sql`) wird im aktuellen Verzeichnis abgelegt.

### Backup wiederherstellen

```bash
# Bestehende Datenbank leeren und Backup einspielen:
docker compose -f compose.prod.yaml exec -T postgres psql -U postgres -d companyOSINT -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"
docker compose -f compose.prod.yaml exec -T postgres psql -U postgres -d companyOSINT < backup_20260220_143000.sql
```

## Logs & Stoppen

| Aktion                      | Befehl                                                      |
|-----------------------------|-------------------------------------------------------------|
| Alle Logs ansehen           | `docker compose -f compose.prod.yaml logs -f`               |
| Nur Web-Logs                | `docker compose -f compose.prod.yaml logs -f findACompany-web` |
| Nur DB-Logs                 | `docker compose -f compose.prod.yaml logs -f postgres`      |
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