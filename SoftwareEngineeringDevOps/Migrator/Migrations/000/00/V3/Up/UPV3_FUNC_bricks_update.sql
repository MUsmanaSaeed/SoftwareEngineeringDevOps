DROP FUNCTION IF EXISTS public.bricks_update(bigint, text, bigint, numeric, text, text, numeric, numeric, numeric, numeric, text, numeric);

create or replace function public.bricks_update(
	"Id" bigint,
	"Name" text,
	"ManufacturerId" bigint,
	"Price" numeric,
	"Colour" text,
	"Material" text,
	"Strength" numeric,
	"Width" numeric,
	"Height" numeric,
	"Depth" numeric,
	"Type" text,
	"Voids" numeric
)
returns setof bricks
as $$
	update bricks
	set name = "Name",
		manufacturerid = "ManufacturerId",
		price = "Price",
		colour = "Colour",
		material = "Material",
		strength = "Strength",
		width = "Width",
		height = "Height",
		depth = "Depth",
		type = "Type",
		voids = "Voids"
	where id = "Id"
	returning *;
$$ language sql;
