namespace MedFarLab.Application.Common.Command
{
    public record PendingCommand(Guid Id,
    string CommandTypeName,
    string EncryptedData,
    DateTime CreatedAt);
}
