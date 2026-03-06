namespace companyOSINT.Worker.Detection;

public enum DetectorKind
{
    Software,
    Tool
}

public record DetectorDescriptor(
    string Name,
    DetectorKind Kind,
    DetectionRule[] Rules,
    DetectorDescriptor? RequiresParent = null);
