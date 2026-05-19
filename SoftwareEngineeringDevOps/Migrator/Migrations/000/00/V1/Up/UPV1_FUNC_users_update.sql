DROP FUNCTION IF EXISTS public.users_update(bigint, text, text, text, text, boolean, boolean);

create or replace function public.users_update(
	"Id" bigint,
	"Username" text,
	"Password" text,
	"FirstName" text,
	"LastName" text,
	"IsAdmin" boolean,
	"IsEditor" boolean
)
returns void
as $$
	update users
	set username = "Username",
	    password = "Password",
	    firstname = "FirstName",
	    lastname = "LastName",
	    isadmin = "IsAdmin",
	    iseditor = "IsEditor"
	where id = "Id";
$$ language sql;