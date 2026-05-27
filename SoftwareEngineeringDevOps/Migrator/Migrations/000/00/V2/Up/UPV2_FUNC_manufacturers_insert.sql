DROP FUNCTION IF EXISTS public.manufacturers_insert(text, text, text, text, text, text);

create or replace function public.manufacturers_insert(
	"Name" text,
	"Address1" text,
	"Address2" text,
	"Postcode" text,
	"PhoneNo" text,
	"Email" text
)
returns setof manufacturers
as $$
	insert into manufacturers(name, address1, address2, postcode, phoneno, email)
	values ("Name", "Address1", "Address2", "Postcode", "PhoneNo", "Email")
	returning *;
$$ language sql;
