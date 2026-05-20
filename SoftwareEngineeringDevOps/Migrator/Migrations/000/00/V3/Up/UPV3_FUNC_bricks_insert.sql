DROP FUNCTION IF EXISTS public.bricks_insert(text, bigint, numeric, text, text, numeric, numeric, numeric, numeric, text, numeric);

create or replace function public.bricks_insert(
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
	insert into bricks(name, manufacturerid, price, colour, material, strength, width, height, depth, type, voids)
	values ("Name", "ManufacturerId", "Price", "Colour", "Material", "Strength", "Width", "Height", "Depth", "Type", "Voids")
	returning *;
$$ language sql;
