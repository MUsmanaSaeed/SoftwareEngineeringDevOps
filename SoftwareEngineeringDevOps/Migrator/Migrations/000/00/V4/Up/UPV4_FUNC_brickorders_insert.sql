DROP FUNCTION IF EXISTS public.brickorders_insert(text, bigint, int, timestamptz, timestamptz, bigint);

create or replace function public.brickorders_insert(
	"OrderNo" text,
	"BrickId" bigint,
	"BricksOrdered" int,
	"OrderedDate" timestamptz,
	"ExpectedDate" timestamptz,
	"CreatedById" bigint
)
returns setof brickorders
as $$
	insert into brickorders(orderno, brickid, bricksordered, ordereddate, expecteddate, createdbyid)
	values ("OrderNo", "BrickId", "BricksOrdered", "OrderedDate", "ExpectedDate", "CreatedById")
	returning *;
$$ language sql;
