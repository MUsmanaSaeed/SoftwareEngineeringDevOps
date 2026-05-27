DROP FUNCTION IF EXISTS public.brickorders_update(bigint, bigint, int, timestamptz, timestamptz);

create or replace function public.brickorders_update(
	"Id" bigint,
	"BrickId" bigint,
	"BricksOrdered" int,
	"OrderedDate" timestamptz,
	"ExpectedDate" timestamptz
)
returns setof brickorders
as $$
	update brickorders
	set brickid = "BrickId",
		bricksordered = "BricksOrdered",
		ordereddate = "OrderedDate",
		expecteddate = "ExpectedDate"
	where id = "Id"
	returning *;
$$ language sql;
