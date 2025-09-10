### Yet another Jira

1. Creating migrations
```bash
dotnet ef migrations add *** --project src/YetAnotherJira.Infrastructure --startup-project src/YetAnotherJira.Web
```

2. Apply migrations
```bash
dotnet ef database update --project src/YetAnotherJira.Infrastructure --startup-project src/YetAnotherJira.Web
```

3. For running in docker
```bash
docker-compose up --build --no-deps -d
```