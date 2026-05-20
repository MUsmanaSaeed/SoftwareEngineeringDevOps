DROP FUNCTION IF EXISTS public.brickorders_insert(text, bigint, int, timestamp, timestamp);

create or replace function public.brickorders_insert(
	"OrderNo" text,
	"BrickId" bigint,
	"BricksOrdered" int,
	"OrderedDate" timestamp,
	"ExpectedDate" timestamp
)
returns setof brickorders
as $$
	insert into brickorders(orderno, brickid, bricksordered, ordereddate, expecteddate)
	values ("OrderNo", "BrickId", "BricksOrdered", "OrderedDate", "ExpectedDate")
	returning *;
$$ language sql;
