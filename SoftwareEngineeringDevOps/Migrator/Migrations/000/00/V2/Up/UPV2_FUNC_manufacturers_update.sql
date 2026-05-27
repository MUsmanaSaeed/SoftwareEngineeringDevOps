DROP FUNCTION IF EXISTS public.manufacturers_update(bigint, text, text, text, text, text, text);

create or replace function public.manufacturers_update(
	"Id" bigint,
	"Name" text,
	"Address1" text,
	"Address2" text,
	"Postcode" text,
	"PhoneNo" text,
	"Email" text
)
returns setof manufacturers
as $$
	update manufacturers
	set name = "Name",
		address1 = "Address1",
		address2 = "Address2",
		postcode = "Postcode",
		phoneno = "PhoneNo",
		email = "Email"
	where id = "Id"
	returning *;
$$ language sql;
