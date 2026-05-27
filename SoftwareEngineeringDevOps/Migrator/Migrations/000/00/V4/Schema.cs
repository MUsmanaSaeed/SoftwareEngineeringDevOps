using FluentMigrator;

namespace Migrator.V4
{
    [Migration(4, "BrickOrders")]
    public class Schema : BaseSchema
    {
        public Schema()
            : base(
            [
                "TABLE_brickorders",
                "FUNC_brickorders_listall",
                "FUNC_brickorders_getbyid",
                "FUNC_brickorders_getbybrickid",
                "FUNC_brickorders_insert",
                "FUNC_brickorders_update",
                "FUNC_brickorders_cancel",
                "FUNC_brickorders_uncancel",
                "FUNC_brickorders_delete",
            ])
        { }
    }
}
