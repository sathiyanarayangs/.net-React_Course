# DATA-COMPARISON.md — Task 4.12

Which of the three approaches fits which situation, based on actually
building the same repository three times this week.

## A stable security table (e.g. Users/roles/permissions)

**Best fit: ADO.NET, or EF Code First with migrations frozen early.**

A security table's shape barely changes once it's designed — Username,
PasswordHash, Role don't grow new columns every sprint the way a product
feature table does. That stability is exactly what makes ADO.NET's extra
hand-written code worth it here: `AdoNetUserRepository` is a handful of
parameterized queries that will basically never need to change, and in
exchange you get full control over exactly what SQL runs against a table
where "exactly what SQL runs" matters more than almost anywhere else in the
app. Stored procedures also make sense for security tables specifically: a
DBA can review and lock down permissions on the procedure itself,
restricting the app's SQL login to "can only call these exact procs"
rather than "can run arbitrary parameterized SQL against this table." EF
Code First is a reasonable second choice if the team is EF-first everywhere
and doesn't want a second data-access pattern just for one table — but the
flexibility EF's change tracking and LINQ provide is mostly wasted on a
table this simple and this static.

## An evolving domain (e.g. the Students table as the course adds features)

**Best fit: EF Code First.**

This is the one Week 4 actually demonstrated directly: Task 4.8 added
`EnrolledOn` to `Student` and generated a second migration in about a
minute, with `Up()`/`Down()` written for me and a clear, reviewable diff of
exactly what changed. Doing the equivalent by hand in ADO.NET means writing
and running an `ALTER TABLE` script myself and remembering to keep
`CreateTables.sql` in sync with reality — nothing enforces that consistency
the way EF's migration history does. For a domain model that's still
actively growing (which "evolving" implies by definition), the cost of Code
First's slightly heavier tooling is paid back the first time you need to
add, rename, or restructure a column and don't want to hand-write the SQL
for it.

## A reporting layer over a database you don't own

**Best fit: EF DB First.**

The instant a database belongs to "another team" (Task 4.9's framing), you
don't get to redesign its schema around your own C# model — Code First's
core idea (model drives schema) is backwards for this situation, since the
schema already exists and already drives everything. DB First scaffolds
entities and a context FROM that existing schema, so the mapping step
(`EfDbFirstStudentRepository`'s `ToStudent`/`ToRow` in this repo) is
explicit and visible, rather than pretending the owned-by-someone-else
table happens to match your own domain model by coincidence. ADO.NET would
also work here (you're just reading someone else's tables with SQL either
way), but a reporting layer usually means a lot of read-heavy LINQ queries
across many tables, and EF's query composition is a lot less to hand-write
than the equivalent ADO.NET DataReader loops for every report.

## The one thing all three have in common

Every observation above is about **where the extra effort is worth
paying**, not about one approach being objectively better. All three
satisfy `IRepository<Student>` identically (Task 4.11 proves this by
swapping the config value and re-running the same Postman collection with
no other changes) — the choice is a cost/fit tradeoff per table, not a
one-time decision for the whole app. A real system could reasonably use
all three at once, exactly the way this repo does.
