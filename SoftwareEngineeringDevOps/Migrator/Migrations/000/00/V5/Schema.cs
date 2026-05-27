using FluentMigrator;

namespace Migrator.V5
{
    [Migration(5, "BrickOrdersReceived")]
    public class Schema : BaseSchema
    {
        public Schema()
            : base(
            [
                "TABLE_brickordersreceived",
                "FUNC_brickordersreceived_listall",
                "FUNC_brickordersreceived_getbyid",
                "FUNC_brickordersreceived_getbybrickorderid",
                "FUNC_brickordersreceived_insert",
                "FUNC_brickordersreceived_update",
                "FUNC_brickordersreceived_delete",
            ])
        { }
    }
}
