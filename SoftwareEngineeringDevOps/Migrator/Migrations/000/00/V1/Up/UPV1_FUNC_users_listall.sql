create or replace function public.users_listall()
returns setof users
as $$
	select * from users;
$$ language sql;