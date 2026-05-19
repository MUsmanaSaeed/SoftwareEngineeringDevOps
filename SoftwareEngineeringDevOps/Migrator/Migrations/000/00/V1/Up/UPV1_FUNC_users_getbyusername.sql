create or replace function public.users_getbyusername(
	"Username" text
)
returns setof users
as $$
	select * from users 
	where username = "Username";
$$ language sql;