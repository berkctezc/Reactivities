# NET 5.0 & React 17 App Project

This is a project for learning purposes named Reactivities.

## 1 - Walking Skeleton Phase

### Required VS Code Extensions & Tips

- **F1:** command palette, **Ctrl+":** terminal
- disable auto save, exclude `**/obj and **/bin` folders in user settings, disable compact folders, add code command to path
- **Extensions:** [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) , [C# Extensions](https://marketplace.visualstudio.com/items?itemName=kreativ-software.csharpextensions) , [NuGet Gallery](https://marketplace.visualstudio.com/items?itemName=patcx.vscode-nuget-gallery) , [SQLite](https://marketplace.visualstudio.com/items?itemName=alexcvzz.vscode-sqlite) , [MSBuild project tools](https://marketplace.visualstudio.com/items?itemName=tintoy.msbuild-project-tools) , [vscode-solution-explorer](https://marketplace.visualstudio.com/items?itemName=fernandoescolar.vscode-solution-explorer)
- from command palette type generate assets and click it. (appsettings.json and appsettings.Development.json files should be created after this operation)
- troubleshooting debugging problems: downgrade c# extension, in settings, "omnisharp.useGlobalMono": "always", "omnisharp.path": "latest"

### Clean Architecture

(API=>(Application=>(Persistence=>(Domain)))

*WIP*

### API - Walking Skeleton

- Create a API and 3 Class libraries named **Application**, **Domain**, **Persistence**
- References should be like **Persistence**: Domain | **Application**: Persistence, Domain
- **DataContext** for EF Core

```cs
namespace Persistence{
    public class DataContext : DbContext {
        public DataContext(DbContextOptions options) : base(options){}
        public DbSet<Activity> Activities { get; set; }
    }
}
```

- Create migration with **dotnet ef**

```bash
dotnet ef migrations add InitialCreate -p Persistence -s API
```

- Add seed data to persistence and run migrations and seed operation on Program.cs

### Client - Walking Skeleton

- Create React App

```bash
npx create-react-app client-app --use-npm --template typescript
```

- **npm start** to run the react project

- `npm install axios`
- Make a request to API using **axios**

```react
import React, { useEffect, useState } from "react";
import logo from "./logo.svg";
import "./App.css";
import axios from "axios";

function App() {
  const [activities, setActivities] = useState([]);

  useEffect(() => {
    axios.get("http://localhost:5000/api/activities").then((response) => {
      console.log(response);
      setActivities(response.data);
    });
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <ul>
          {activities.map((activity:any)=>(
            <li key={activity.id}>
              {activity.title}
            </li>
          ))}
        </ul>
      </header>
    </div>
  );
}

export default App;
```

- Add Cors Policy to API Startup

```cs
        public void ConfigureServices(IServiceCollection services)
        {
			//...
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000");
                });
            });
        }
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();
		   //...
        }
```

- Install Semantic UI `npm install semantic-ui-react semantic-ui-css`
- Import in index.ts `import 'semantic-ui-css/semantic.min.css'`
- Use Semantic UI elements instead of built-in ones in App.tsx

```react
  return (
    <div>
      <Header as='h2' icon='users' content='Reactivities'/>
        <List>
          {activities.map((activity:any)=>(
            <List.Item key={activity.id}>
              {activity.title}
            </List.Item>
          ))}
        </List>
    </div>
  );
}
```

