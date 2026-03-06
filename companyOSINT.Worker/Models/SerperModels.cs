namespace companyOSINT.Worker.Models;

public record SerperResponse(List<SerperOrganicResult> Organic);

public record SerperOrganicResult(string Title, string Link, string Snippet);
