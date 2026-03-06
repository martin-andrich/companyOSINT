using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Detection;

public record DetectionResult(
    List<SoftwareDetection> Software,
    List<ToolDetection> Tools);
