create or replace function public.brickorders_cancel(
	"Id" bigint,
	"CancelledDate" timestamptz
)
returns setof brickorders
as $$
	update brickorders
	set cancelleddate = "CancelledDate"
	where id = "Id"
	returning *;
$$ language sql;
