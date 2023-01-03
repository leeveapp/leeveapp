namespace Leeve.Server.Common;

public class BaseDocument : Entity, ICreatedOn, IModifiedOn, IDeleteOn
{
    public CreatedBy? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public ModifiedBy? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool SystemGenerated { get; set; }
    public bool Deleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public DeletedBy? DeletedBy { get; set; }
}