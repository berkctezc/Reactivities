# NET 6.0 & React 17 App Project

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

```tsx
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
          {activities.map((activity: any) => (
            <li key={activity.id}>{activity.title}</li>
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
        public void ConfigureServices(IServiceCollection services){
   //...
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000");
                });
            });
        }
 public void Configure(IApplicationBuilder app, IWebHostEnvironment env){
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

```tsx
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

## 2 - CQRS and Mediator Pattern

- Install **Mediatr.Extensions.Microsoft.DependencyInjection** to **Application** layer
- Install **Mediatr** in **API** layer
- Create query and handler in Application layer

```cs
public class List
{
    public class Query : IRequest<List<Activity>> { }

    public class Handler : IRequestHandler<Query, List<Activity>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Activity>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _context.Activities.ToListAsync(cancellationToken: cancellationToken);
        }
    }
}
```

- Update controller. Inject IMediator and

```cs
[HttpGet]
public async Task<ActionResult<List<Activity>>> GetActivities()
{
    return await _mediator.Send(new List.Query());
}
```

- Add `services.AddMediatR(typeof(List.Handler).Assembly);` to **Startup ConfigureServices**

- To thin controllers, inject IMediator in **BaseApiController** and remove **ctor** from individual controllers

```cs
   [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
    }
```

- and use like this

```cs
[HttpGet]
public async Task<ActionResult<List<Activity>>> GetActivities()
{
    return await Mediator.Send(new List.Query());
}
```

### Adding other handlers

- Details

```cs
namespace Application.Activities
{
    public class Details
    {
        public class Query : IRequest<Activity>
        {
            public Guid Id { get; set; }
        }
        public class Handler : IRequestHandler<Query, Activity>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Activity> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Activities.FindAsync(request.Id);
            }
        }
    }
}
//...
namespace API.Controllers
{
    public class ActivitiesController : BaseApiController
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<Activity>> GetActivity(Guid id)
        {
            return await Mediator.Send(new Details.Query() { Id = id });
        }
    }
}
```

- Create

```cs
namespace Application.Activities
{
    public class Create
    {
        public class Command : IRequest
        {
            public Activity Activity { get; set; }
        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                await _context.Activities.AddAsync(request.Activity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}
//...
namespace API.Controllers
{
    public class ActivitiesController : BaseApiController
    {
        [HttpPost]
        public async Task<IActionResult> CreateActivity(Activity activity)
        {
            return Ok(await Mediator.Send(new Create.Command { Activity = activity }));
        }
    }
}
```

- Edit

```cs
namespace Application.Activities
{
    public class Edit
    {
        public class Command : IRequest
        {
            public Activity Activity { get; set; }
        }
        public class Handler : IRequestHandler<Command>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var activityToUpdate = await _context.Activities.FindAsync(request.Activity.Id);
                _mapper.Map(request.Activity, activityToUpdate);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}
//...
namespace API.Controllers
{
    public class ActivitiesController : BaseApiController
    {
        [HttpPut("{id}")]
        public async Task<IActionResult> EditActivity(Guid id, Activity activity)
        {
            activity.Id = id;
            return Ok(await Mediator.Send(new Edit.Command { Activity = activity }));
        }
    }
}

```

- Delete

```cs
namespace Application.Activities{
    public class Delete{
        public class Command : IRequest{
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command>{
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper){
                _context = context;
                _mapper = mapper;
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken){
                var activityToDelete = await _context.Activities.FindAsync(request.Id);
                _context.Remove(activityToDelete);
                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;
            }
        }
    }
}
//...
namespace API.Controllers{
    public class ActivitiesController : BaseApiController{
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id){
            return Ok(await Mediator.Send(new Delete.Command { Id = id }));
        }
    }
}
```

### Adding AutoMapper

- Install AutoMapper `AutoMapper.Extensions.Microsoft.DependencyInjection` to Application layer
- Create MappingProfile in Application Layer, Core folder

```cs
namespace Application.Core{
    public class MappingProfiles : Profile{
        public MappingProfiles(){
            CreateMap<Activity, Activity>();
        }
    }
}
```

- And in **Startup.cs** add `services.AddAutoMapper(typeof(MappingProfiles).Assembly);` to **ConfigureServices**

### Tidying up Startup

- Create a Extensions folder in API layer and create **ApplicationServiceExtensions**

```cs
namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSwaggerGen(c =>{
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });
            services.AddDbContext<DataContext>(opt =>{
                opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            services.AddCors(opt =>            {
                opt.AddPolicy("CorsPolicy", policy =>{
                    policy.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000");
                });
            });
            services.AddMediatR(typeof(List.Handler).Assembly);
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            return services;
        }
    }
}
```

- Remove these lines from Startup, ConfigureService and add `services.AddApplicationServices(_config);`

## 3 - React

### Folder structure in react

- React does not make you use a predefined structure. You are completely free but you should use some structure for everyone's sanity
- We are going to use feature based structure

```bash
src/
	app/ #not specific to any feature
		layout/
			App.tsx
			styles.css
	features/

```

_don't forget to update imports_

### Adding an Activity Interface

- Copy Swagger activity get response and convert it by using json to ts tool

```ts
export interface Activity {
  id: string;
  title: string;
  date: string;
  description: string;
  category: string;
  city: string;
  venue: string;
}
```

- Add type to activities useState `const [activities, setActivities] = useState<Activity[]>([]);`
- remove any from map function. Now we get errors if we made mistakes and get completions.
- Add type to axios get request `axios.get<Activity[]>('http://localhost:5000/api/activities').then(response=>{setActivities(response.data);})`,

### Adding a Nav bar

- Create NavBar.tsx in layout folder. Use semantic ui tags

```tsx
import { Button, Container, Menu } from "semantic-ui-react";

interface Props {
  openForm: () => void;
}

export default function NavBar({ openForm }: Props) {
  return (
    <Menu inverted fixed="top">
      <Container>
        <Menu.Item header>
          <img
            src="/assets/logo.png"
            alt="logo"
            style={{ marginRight: "10px" }}
          />
          Reactivities
        </Menu.Item>
        <Menu.Item name="Activities" />
        <Menu.Item>
          <Button onClick={openForm} positive content="Create Activity" />
        </Menu.Item>
      </Container>
    </Menu>
  );
}
```

- Styles

```css
body {
  background-color: #eaeaea !important;
}

.ui.inverted.top.fixed.menu {
  background-image: linear-gradient(
    90deg,
    rgba(150, 15, 23, 1) 0%,
    rgba(1, 148, 177, 1) 100%
  ) !important;
}
```

- Add to App.tsx

```tsx
return (
    <>
    <NavBar/>
    <Container style={{ marginTop: "5em" }}>
    </Container>
      );
```

### Creating an activity dashboard

- Create a dashboard directory in activities folder and Create **ActivityDashboard.tsx** component

```tsx
WI;
```

## Axios

_WIP_

## MobX

#### Setting up MobX

- `npm install mobx mobx-react-lite`
- Create a folder named stores - for storing stores in one central location
- Create class activityStore.ts in folder store

```tsx
export default class ActivitStore {
  title = "Hello from MobX!";

  constructor() {
    makeObservable(this, {
      title: observable,
    });
  }
}
```

```tsx
- Create an interface store

import { createContext, useContext } from 'react';
import ActivitStore from './activityStore';
interface Store {
    activityStore: ActivitStore
}

export const store: Store = {
    activityStore: new ActivitStore()
}

export const StoreContext = createContext(store);

export function useStore() {
    return useContext(StoreContext);
}
```

- Provide context to our application in index.tsx

```tsx
<StoreContext.Provider value={store}>
  <App />
</StoreContext.Provider>
```

- Add activityStore to App.tsx and access it in function

```tsx
function App() {
  const { activityStore } = useStore();
  //...
   return (
    <>
      <NavBar openForm={handleFormOpen} />
      <Container style={{ marginTop: "5em" }}>
		<!--... -->
        <h2>{activityStore.title}</h2>
      </Container>
    </>
  );
```

#### MobX actions

- Add action to **activityStore**, also changed makeObservable to makeAutoObservable. It detects properties and makes them observable, and detects functions to make them actions.

```ts
constructor() {
    makeAutoObservable(this)
    }    
setTitle = () => {
        this.title = this.title + '!';
    }
```

- Using in view (**App.tsx**)

```tsx
<!--...-->
<Button content='Add exclamation!' positive onClick={activityStore.setTitle}></Button>

export default observer(App);
```

#### Refactoring code for MobX

- Moved activities variable and logic from state of the component to store. 
- **App.tsx**

```tsx
  useEffect(() => {
    activityStore.loadActivities();
  }, [activityStore]);
<!--...-->
  return (
    <>
      <NavBar openForm={handleFormOpen} />
      <Container style={{ marginTop: "5em" }}>
        <ActivityDashboard
          activities={activityStore.activities}
        />
    <!--...-->
      </Container>
    </>
  );
}

export default observer(App);
```

- **activityStore.ts**

```ts
export default class ActivityStore {
    activities: Activity[] = [];
    selectedActivity: Activity | null = null;
    editMode = false;
    loading = false;
    loadingInitial = false;
    constructor() {
        makeAutoObservable(this)
    }
    loadActivities = async () => {
        this.loadingInitial = true;
        try {
            const activities = await agent.Activities.list();
            activities.forEach(activity => {
                activity.date = activity.date.split('T')[0];
                this.activities.push(activity);
            })
            this.loadingInitial = false;
        } catch (error) {
            console.log(error);
            this.loadingInitial = false;
        }
    }
}
```

