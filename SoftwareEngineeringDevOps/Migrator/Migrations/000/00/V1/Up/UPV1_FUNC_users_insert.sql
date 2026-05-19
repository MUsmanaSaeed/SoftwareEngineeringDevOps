DROP FUNCTION IF EXISTS public.users_insert(text, text, text, text, boolean, boolean);

create or replace function public.users_insert(
	"Username" text,
	"Password" text,
	"FirstName" text,
	"LastName" text,
	"IsAdmin" boolean,
	"IsEditor" boolean
)
returns void
as $$
	insert into users(username, password, firstname, lastname, isadmin, iseditor)
	values ("Username", "Password", "FirstName", "LastName", "IsAdmin", "IsEditor");
$$ language sql;