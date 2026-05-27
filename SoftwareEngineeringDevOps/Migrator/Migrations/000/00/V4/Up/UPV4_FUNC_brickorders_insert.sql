DROP FUNCTION IF EXISTS public.brickorders_insert(text, bigint, int, timestamp, timestamp, bigint);

create or replace function public.brickorders_insert(
	"OrderNo" text,
	"BrickId" bigint,
	"BricksOrdered" int,
	"OrderedDate" timestamp,
	"ExpectedDate" timestamp,
	"CreatedById" bigint
)
returns setof brickorders
as $$
	insert into brickorders(orderno, brickid, bricksordered, ordereddate, expecteddate, createdbyid)
	values ("OrderNo", "BrickId", "BricksOrdered", "OrderedDate", "ExpectedDate", "CreatedById")
	returning *;
$$ language sql;
