# NET 6.0 & React 17 App Project

This is a project for learning purposes named Reactivities.

## 1 - Walking Skeleton Phase

### Required VS Code Extensions & Tips

- **F1:** command palette, **Ctrl+":** terminal
- disable auto save, exclude `**/obj and **/bin` folders in user settings, disable compact folders, add code command to
  path
- **Extensions:** [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
  , [C# Extensions](https://marketplace.visualstudio.com/items?itemName=kreativ-software.csharpextensions)
  , [NuGet Gallery](https://marketplace.visualstudio.com/items?itemName=patcx.vscode-nuget-gallery)
  , [SQLite](https://marketplace.visualstudio.com/items?itemName=alexcvzz.vscode-sqlite)
  , [MSBuild project tools](https://marketplace.visualstudio.com/items?itemName=tintoy.msbuild-project-tools)
  , [vscode-solution-explorer](https://marketplace.visualstudio.com/items?itemName=fernandoescolar.vscode-solution-explorer)
- from command palette type generate assets and click it. (appsettings.json and appsettings.Development.json files
  should be created after this operation)
- troubleshooting debugging problems: downgrade c# extension, in settings, "omnisharp.useGlobalMono": "always", "
  omnisharp.path": "latest"

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
import React, {useEffect, useState} from "react";
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
                <img src={logo} className="App-logo" alt="logo"/>
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
            {activities.map((activity: any) => (
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

- React does not make you use a predefined structure. You are completely free but you should use some structure for
  everyone's sanity
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
- Add type to axios get
  request `axios.get<Activity[]>('http://localhost:5000/api/activities').then(response=>{setActivities(response.data);})`
  ,

### Adding a Nav bar

- Create NavBar.tsx in layout folder. Use semantic ui tags

```tsx
import {Button, Container, Menu} from "semantic-ui-react";

interface Props {
    openForm: () => void;
}

export default function NavBar({openForm}: Props) {
    return (
        <Menu inverted fixed="top">
            <Container>
                <Menu.Item header>
                    <img
                        src="/assets/logo.png"
                        alt="logo"
                        style={{marginRight: "10px"}}
                    />
                    Reactivities
                </Menu.Item>
                <Menu.Item name="Activities"/>
                <Menu.Item>
                    <Button onClick={openForm} positive content="Create Activity"/>
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
        <Container style={{marginTop: "5em"}}>
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

- Create an interface store
  
  ```tsx
  
  ```

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
    const {activityStore} = useStore();
    //...
    return (
        <>
            <NavBar openForm={handleFormOpen}/>
            <Container style={{marginTop: "5em"}}>
                <!--... -->
                <h2>{activityStore.title}</h2>
            </Container>
        </>
    );
```

#### MobX actions

- Add action to **activityStore**, also changed makeObservable to makeAutoObservable. It detects properties and makes
  them observable, and detects functions to make them actions.

```ts
constructor()
{
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
        <NavBar openForm={handleFormOpen}/>
        <Container style={{marginTop: "5em"}}>
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
        makeAutoObservable(this);
    }

    loadActivities = async () => {
        this.loadingInitial = true;
        try {
            const activities = await agent.Activities.list();
            activities.forEach((activity) => {
                activity.date = activity.date.split("T")[0];
                this.activities.push(activity);
            });
            this.loadingInitial = false;
        } catch (error) {
            console.log(error);
            this.loadingInitial = false;
        }
    };
}
```

#### Selecting an activity using MobX and strict mode

- Big changes
- **App.tsx**

```tsx
function App() {
    const {activityStore} = useStore();

    const [activities, setActivities] = useState<Activity[]>([]);
    const [setSelectedActivity] = useState<Activity | undefined>(undefined);
    const [setEditMode] = useState(false);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        activityStore.loadActivities();
    }, [activityStore]);

    function handleCreateOrEditActivity(activity: Activity) {
        setSubmitting(true);
        if (activity.id) {
            agent.Activities.update(activity).then(() => {
                setActivities([
                    ...activities.filter((x) => x.id !== activity.id),
                    activity,
                ]);
                setSubmitting(false);
            });
        } else {
            activity.id = uuid();
            agent.Activities.create(activity).then(() => {
                setActivities([...activities, activity]);
                setSubmitting(false);
            });
        }
    }

    function handleDeleteActivity(id: string) {
        setSubmitting(true);
        agent.Activities.delete(id).then(() => {
            setActivities([...activities.filter((x) => x.id !== id)]);
            setSubmitting(false);
        });
    }

    if (activityStore.loadingInitial)
        return <LoadingComponent content="Loading app"/>;

    return (
        <>
            <NavBar/>
            <Container style={{marginTop: "5em"}}>
                <ActivityDashboard
                    activities={activityStore.activities}
                    createOrEdit={handleCreateOrEditActivity}
                    deleteActivity={handleDeleteActivity}
                    submitting={submitting}
                />
            </Container>
        </>
    );
}

export default observer(App);
```

- Navbar.tsx

```tsx
export default function NavBar() {
    const {activityStore} = useStore();

    return (
        <Menu inverted fixed="top">
            <Container>
                <Menu.Item header>
                    <img
                        src="/assets/logo.png"
                        alt="logo"
                        style={{marginRight: "10px"}}
                    />
                    Reactivities
                </Menu.Item>
                <Menu.Item name="Activities"/>
                <Menu.Item>
                    <Button
                        onClick={() => activityStore.openForm()}
                        positive
                        content="Create Activity"
                    />
                </Menu.Item>
            </Container>
        </Menu>
    );
}
```

- activityStore.ts

```ts
export default class ActivityStore {
    activities: Activity[] = [];
    selectedActivity: Activity | undefined = undefined;
    editMode = false;
    loading = false;
    loadingInitial = false;

    constructor() {
        makeAutoObservable(this);
    }

    loadActivities = async () => {
        this.setLoadingInitial(true);
        try {
            const activities = await agent.Activities.list();
            activities.forEach((activity) => {
                activity.date = activity.date.split("T")[0];
                this.activities.push(activity);
            });
            this.setLoadingInitial(false);
        } catch (error) {
            console.log(error);
            this.setLoadingInitial(false);
        }
    };
    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    };
    selectActivity = (id: string) => {
        this.selectedActivity = this.activities.find((a) => a.id === id);
    };
    cancelSelectedActivity = () => {
        this.selectedActivity = undefined;
    };
    openForm = (id?: string) => {
        id ? this.selectActivity(id) : this.cancelSelectedActivity();
        this.editMode = true;
    };
    closeForm = () => {
        this.editMode = false;
    };
}
```

- ActivityDashboard.tsx

```tsx
interface Props {
    activities: Activity[];
    createOrEdit: (activity: Activity) => void;
    deleteActivity: (id: string) => void;
    submitting: boolean;
}

export default observer(function ActivityDashboard({
                                                       activities,
                                                       deleteActivity,
                                                       createOrEdit,
                                                       submitting,
                                                   }: Props) {
    const {activityStore} = useStore();
    const {selectedActivity, editMode} = activityStore;

    return (
        <Grid>
            <Grid.Column width="10">
                <ActivityList
                    activities={activities}
                    deleteActivity={deleteActivity}
                    submitting={submitting}
                />
            </Grid.Column>
            <Grid.Column width="6">
                {selectedActivity && !editMode && <ActivityDetails/>}
                {editMode && (
                    <ActivityForm createOrEdit={createOrEdit} submitting={submitting}/>
                )}
            </Grid.Column>
        </Grid>
    );
});
```

- ActivityList.tsx

```tsx
interface Props {
    activities: Activity[];
    createOrEdit: (activity: Activity) => void;
    deleteActivity: (id: string) => void;
    submitting: boolean;
}

export default observer(function ActivityDashboard({
                                                       activities,
                                                       deleteActivity,
                                                       createOrEdit,
                                                       submitting,
                                                   }: Props) {
    const {activityStore} = useStore();
    const {selectedActivity, editMode} = activityStore;

    return (
        <Grid>
            <Grid.Column width="10">
                <ActivityList
                    activities={activities}
                    deleteActivity={deleteActivity}
                    submitting={submitting}
                />
            </Grid.Column>
            <Grid.Column width="6">
                {selectedActivity && !editMode && <ActivityDetails/>}
                {editMode && (
                    <ActivityForm createOrEdit={createOrEdit} submitting={submitting}/>
                )}
            </Grid.Column>
        </Grid>
    );
});
```

- ActivityDetailst.tsx

```tsx
export default function ActivityDetails() {
    const {activityStore} = useStore();
    const {
        selectedActivity: activity,
        openForm,
        cancelSelectedActivity,
    } = activityStore;

    if (!activity) return <LoadingComponent content={""}/>;

    return (
        <Card fluid>
            <Image src={`/assets/categoryImages/${activity.category}.jpg`}/>
            <Card.Content>
                <Card.Header>{activity.title}</Card.Header>
                <Card.Meta>
                    <span>{activity.date}</span>
                </Card.Meta>
                <Card.Description>{activity.description}</Card.Description>
            </Card.Content>
            <Card.Content>
                <Button.Group widths="2">
                    <Button
                        onClick={() => openForm(activity.id)}
                        basic
                        color="blue"
                        content="edit"
                    />
                    <Button
                        onClick={cancelSelectedActivity}
                        basic
                        color="grey"
                        content="cancel"
                    />
                </Button.Group>
            </Card.Content>
        </Card>
    );
}
```

- ActivityForm.tsx

```tsx
export default function ActivityForm({createOrEdit, submitting}: Props) {
    const {activityStore} = useStore();
    const {selectedActivity, closeForm} = activityStore;
    //...
}
```

#### Create and edit with MobX

- Removed handleCreateOrEditActivity from **App.tsx**
- Removed createOrEdit from **ActivityDashboard.tsx**
- Added createActivity and updateActivity functions to **activityStore.ts**

```ts
createActivity = async (activity: Activity) => {
    this.loading = true;
    activity.id = uuid();
    try {
        await agent.Activities.create(activity);
        runInAction(() => {
            this.activities.push(activity);
            this.selectedActivity = activity;
            this.editMode = false;
            this.loading = false;
        });
    } catch (error) {
        console.log(error);
        runInAction(() => {
            this.loading = false;
        });
    }
};

updateActivity = async (activity: Activity) => {
    this.loading = true;
    try {
        await agent.Activities.update(activity);
        runInAction(() => {
            this.activities = [
                ...this.activities.filter((a) => a.id !== activity.id),
            ];
            this.selectedActivity = activity;
            this.editMode = false;
            this.loading = false;
        });
    } catch (error) {
        console.log(error);
        runInAction(() => {
            this.loading = false;
        });
    }
};
```

- Updated **ActivityForm.tsx**. Removed props and changed handleSubmit logic

```tsx
function handleSubmit() {
    activity.id ? updateActivity(activity) : createActivity(activity);
}
```

#### Delete with MobX

- Added functionality to **activityStore.ts**

```ts
deleteActivity = async (id: string) => {
    this.loading = true;
    try {
        await agent.Activities.delete(id);
        runInAction(() => {
            this.activities = [...this.activities.filter((a) => a.id !== id)];
            if (this.selectedActivity?.id === id) this.cancelSelectedActivity();
            this.loading = false;
        });
    } catch (error) {
        console.log(error);
        runInAction(() => {
            this.loading = false;
        });
    }
};
```

- Cleanup of props in **ActivityDashboard.tsx** and **App.tsx**

- Cleanup and useStore in **ActivityList.tsx**

```tsx
export default observer(function ActivityList() {
    const {activityStore} = useStore();
    const {deleteActivity, activities, loading} = activityStore;
//...
    return (
        <Segment>
            <!--...-->
            <Button
                name={activity.id}
                loading={loading && target === activity.id}
                onClick={(e) => handleActivityDelete(e, activity.id)}
                floated='right'
                content='Delete'
                color='red'/>
            <!--...-->
        </Segment>
    )
})
```

#### Refactoring the code by using Javascript map object to store the activities

- **activityStore.ts** changes

```ts
export default class ActivityStore {
    activityRegistry = new Map<string, Activity>();

    //...
    get activitiesByDate() {
        return Array.from(this.activityRegistry.values()).sort(
            (a, b) => Date.parse(a.date) - Date.parse(b.date)
        );
    }

    //...
    loadActivities = async () => {
        try {
            const activities = await agent.Activities.list();
            activities.forEach((activity) => {
                activity.date = activity.date.split("T")[0];
                this.activityRegistry.set(activity.id, activity);
            });
            this.setLoadingInitial(false);
        } catch (error) {
            console.log(error);
            this.setLoadingInitial(false);
        }
    };
    //...
    selectActivity = (id: string) => {
        this.selectedActivity = this.activityRegistry.get(id);
    };
    //...
    createActivity = async (activity: Activity) => {
        this.loading = true;
        activity.id = uuid();
        try {
            await agent.Activities.create(activity);
            runInAction(() => {
                this.activityRegistry.set(activity.id, activity);
                this.selectedActivity = activity;
                this.editMode = false;
                this.loading = false;
            });
        } catch (error) {
            console.log(error);
            runInAction(() => {
                this.loading = false;
            });
        }
    };
    //...
    deleteActivity = async (id: string) => {
        this.loading = true;
        try {
            await agent.Activities.delete(id);
            runInAction(() => {
                this.activityRegistry.delete(id);
                if (this.selectedActivity?.id === id) this.cancelSelectedActivity();
                this.loading = false;
            });
        } catch (error) {
            console.log(error);
            runInAction(() => {
                this.loading = false;
            });
        }
    };
}
```

- Added activitiesByDate to activity Store deconstructor in **ActivityList.tsx** and changed it in view.

## React Router

#### Installing react router

- `npm install react-router-dom@5.3.0`
  `npm install @types/react-router-dom@5.3.2 --save-dev`

- Wrap App with BrowserRouter in **index.tsx**

```tsx
<BrowserRouter>
    <App/>
</BrowserRouter>
```

#### Adding Routers

- Added **HomePage.tsx** for route example

```tsx
import {Container} from "semantic-ui-react";

export default function HomePage() {
    return (
        <Container style={{marginTop: '7em'}}>
            <h1>Home Page</h1>
        </Container>
    )
}
```

- Specified routes in **App.tsx**

```tsx
<NavBar/>
<Container style={{marginTop: "5em"}}>
    <Routes>
        <Route path='/' element={<HomePage/>}/>
        <Route path='/activities' element={<ActivityDashboard/>}/>
        <Route path='/createActivity' element={<ActivityForm/>}/>
    </Routes>
</Container>
```

- Moved loading to ActivityDashboard

```tsx
useEffect(() => {
    activityStore.loadActivities();
}, [activityStore]);

if (activityStore.loadingInitial) return <LoadingComponent content="Loading app"/>
```

#### Adding Navlinks

- Added navlinks to navbar

```tsx
<Container>
    <Menu.Item as={NavLink} to='/' header>
        <img src="/assets/logo.png" alt="logo" style={{marginRight: '10px'}}/>
        Reactivities
    </Menu.Item>
    <Menu.Item as={NavLink} to='/activities' name="Activities"/>
    <Menu.Item>
        <Button as={NavLink} to='createActivity' positive content='Create Activity'/>
    </Menu.Item>
</Container>
```

#### Details Link

- Added route to **App.tsx**

```tsx
<Route path='/activities:id' element={<ActivityDetails/>}/>
```

- Added Link to **ActivityList.tsx**

```tsx
<Button as={Link} to={`/activities/${activity.id}`} floated='right' content='View' color='blue'/>
```

#### Getting an individual activity

- Removed setLoadingInitial, selectActivity, cancelSelectedActivity, openForm, closeForm from **activityStore.ts** and
  added these.

```ts
loadActivities = async () => {
    try {
        this.loadingInitial = true;
        const activities = await agent.Activities.list();
        activities.forEach(activity => {
            this.setActivity(activity);
        })
        this.setLoadingInitial(false);
    } catch (error) {
        console.log(error);
        this.setLoadingInitial(false);
    }
}
loadActivity = async (id: string) => {
    let activity = this.getActivity(id);
    if (activity) {
        this.selectedActivity = activity;
    } else {
        this.loadingInitial = true;
        try {
            activity = await agent.Activities.details(id);
            this.setActivity(activity);
            this.selectedActivity = activity;
            this.setLoadingInitial(false);
        } catch (error) {
            console.log(error);
            this.setLoadingInitial(false);
        }
    }
}
private
setActivity = (activity: Activity) => {
    activity.date = activity.date.split('T')[0];
    this.activityRegistry.set(activity.id, activity);
}
private
getActivity = (id: string) => {
    return this.activityRegistry.get(id);
}
setLoadingInitial = (state: boolean) => {
    this.loadingInitial = state;
}
```

- removed references to deleted functions in **ActivityForm**, **ActivityDetails**

#### Using route parameters

- add id parameter to activity details, useEffect and made function an observer

```tsx
export default observer(function ActivityDetails() {
    const {activityStore} = useStore();
    const {selectedActivity: activity, loadActivity, loadingInitial} = activityStore;
    const {id} = useParams<{ id: string }>();

    useEffect(() => {
        if (id) loadActivity(id);
    }, [id, loadActivity]);

    if (loadingInitial || !activity) return <LoadingComponent/>;
//...
```

#### Adding the edit activity route

- modify App.tsx create activity route

```tsx
<Route key={location.key} path={['/createActivity', '/manage/:id']} component={ActivityForm}/>
```

- changed edit and cancel to links in **ActivityDetails**

```tsx
<Button.Group widths='2'>
    <Button as={Link} to={`/manage/${activity.id}`} basic color='blue' content='Edit'/>
    <Button as={Link} to='/activities' basic color='grey' content='Cancel'/>
</Button.Group>
```

- add default state to **ActivityForm** if no id for create

```tsx
export default observer(function ActivityForm() {
    const history = useHistory();
    const {activityStore} = useStore();
    const {
        createActivity, updateActivity,
        loading, loadActivity, loadingInitial
    } = activityStore;
    const {id} = useParams<{ id: string }>();

    const [activity, setActivity] = useState({
        id: '',
        title: '',
        category: '',
        description: '',
        date: '',
        city: '',
        venue: ''
    });

    useEffect(() => {
        if (id) loadActivity(id).then(activity => setActivity(activity!))
    }, [id, loadActivity]);
    //...

    if (loadingInitial) return <LoadingComponent content='Loading activity...'/>
```

- return activity in **activityStore **loadActivity method

```ts
loadActivity = async (id: string) => {
    let activity = this.getActivity(id);
    if (activity) {
        this.selectedActivity = activity;
        return activity;
    } else {
        this.loadingInitial = true;
        try {
            activity = await agent.Activities.details(id);
            this.setActivity(activity);
            runInAction(() => {
                this.selectedActivity = activity;
            })
            this.setLoadingInitial(false);
            return activity;
        } catch (error) {
            console.log(error);
            this.setLoadingInitial(false);
        }
    }
}
```

#### Adding a key to the route

-

- Add location to **App.tsx**
  
  ```tsx
  function App() {
    const location = useLocation();
  
    return (
      <>
        <Route exact path='/' component={HomePage} />
        <Route
          path={'/(.+)'}
          render={() => (
            <>
              <NavBar />
              <Container style={{ marginTop: '7em' }}>
               //...
                <Route key={location.key} path={['/createActivity ', '/manage/:id']} component={ActivityForm} />
              </Container>
           //...
  ```

#### Redirect after submission

- moved uuid generation from activity store to **ActivityForm**

- updated handleSubmit in **ActivityForm**

```tsx
    function handleSubmit() {
    if (activity.id.length === 0) {
        let newActivity = {
            ...activity,
            id: uuid()
        };
        createActivity(newActivity).then(() => history.push(`/activities/${newActivity.id}`))
    } else {
        updateActivity(activity).then(() => history.push(`/activities/${activity.id}`))
    }
}
```

#### Moving the home page outside of nav

- in **App.tsx**

```tsx
function App() {
    const location = useLocation();

    return (
        <>
            <Route exact path='/' component={HomePage}/>
            <Route
                path={'/(.+)'}
                render={() => (
                    <>
                        <NavBar/>
                        <Container style={{marginTop: '7em'}}>
                            <Route exact path='/activities' component={ActivityDashboard}/>
                            <Route path='/activities/:id' component={ActivityDetails}/>
                            <Route key={location.key} path={['/createActivity ', '/manage/:id']}
                                   component={ActivityForm}/>
                        </Container>
                    </>
                )}
            />
        </>
    );
}
```

- Add link to activities in HomePage

```tsx
 return (
    <Container style={{marginTop: '7em'}}>
        <h1>Home page</h1>
        <h3>Go to <Link to='/activities'>Activities</Link></h3>
    </Container>
)
```

#### Styling the activity list

- moved activity list item out of ActivityList to ActivityListItem
  
  ```tsx
  interface Props {
      activity: Activity
  }
  
  export default function ActivityListItem({activity}: Props) {
      const { activityStore } = useStore();
      const { deleteActivity, loading } = activityStore;
  
      const [target, setTarget] = useState('');
  
      function handleActivityDelete(e: SyntheticEvent<HTMLButtonElement>, id: string) {
          setTarget(e.currentTarget.name);
          deleteActivity(id);
      }
  
      return (
          <Item key={activity.id}>
              <Item.Content>
                  <Item.Header as='a'>{activity.title}</Item.Header>
                  <Item.Meta>{activity.date}</Item.Meta>
                  <Item.Description>
                      <div>{activity.description}</div>
                      <div>{activity.city}, {activity.venue}</div>
                  </Item.Description>
                  <Item.Extra>
                      <Button as={Link} to={`/activities/${activity.id}`} floated='right' content='View' color='blue' />
                      <Button
                          name={activity.id}
                          loading={loading && target === activity.id}
                          onClick={(e) => handleActivityDelete(e, activity.id)}
                          floated='right'
                          content='Delete'
                          color='red'
                      />
                      <Label basic content={activity.category} />
                  </Item.Extra>
              </Item.Content>
          </Item>
      )
  }
  ```

#### Grouping the activities by date

- Added groupedActivities function to activityStore
  
  ```tsx
  get groupedActivities() {
     return Object.entries(
        this.activitiesByDate.reduce((activities, activity)=>{
           const date = activity.date;
           activities[date] = activities[date] ? [...activities[date], activity] : [activity];
           return activities;
     },{} as {[key: string]: Activity[]})
    )
  }
  ```

- updated ActivityList to use groupedActivities

```tsx
 return (
    <>
        {groupedActivities.map(([group, activities]) => (
            <Fragment key={group}>
                <Header sub color='teal'>
                    {group}
                </Header>
                <Segment>
                    <Item.Group divided>
                        {activities.map(activity => (
                            <ActivityListItem key={activity.id} activity={activity}/>
                        ))}
                    </Item.Group>
                </Segment>
            </Fragment>
        ))}
    </>
)
```

#### Styling the list items

- ActivityListItem refactor, removed Segment and ItemGroup tags from ActivityList
  
  ```tsx
  return (
          <Segment.Group>
              <Segment>
                  <Item.Group>
                      <Item>
                          <Item.Image size='tiny' circular src='https://picsum.photos/128'></Item.Image>
                          <Item.Content>
                              <Item.Header as={Link} to={`/activites/${activity.id}`}>
                                  {activity.title}
                              </Item.Header>
                              <Item.Description>
                                  Hosted by 😺
                              </Item.Description>
                          </Item.Content>
                      </Item>
                  </Item.Group>
              </Segment>
              <Segment>
                  <span>
                      <Icon name='clock' />{activity.date}
                      <Icon name='marker' /> {activity.venue}
                  </span>
              </Segment>
              <Segment secondary>
                  Attendees go here
              </Segment>
              <Segment clearing>
                  <span>{activity.description}</span>
                  <Button as={Link} to={`/activities/${activity.id}`}
                      color='teal' floated='right' content='View' />
              </Segment>
          </Segment.Group>
      )
  ```

#### Activity details page

- refactored ActivityDetails seperated into components in folder features/activities/details

```tsx
    return (
    <Grid>
        <Grid.Column width={10}>
            <ActivityDetailedHeader/>
            <ActivityDetailedInfo/>
            <ActivityDetailedChat/>
        </Grid.Column>
        <Grid.Column width={6}>
            <ActivityDetailedSidebar/>
        </Grid.Column>
    </Grid>
)
})
```

#### Populating detailed components

- populated from snippets

#### Adding the activity filter component

- created file in dashboard folder ActivityFilters, replaced placeholder h2 from ActivityDashboard with ActivityFilters

- npm install react-calendar

```tsx
    export default function ActivityFilters() {
    return (
        <><Menu vertical size='large' style={{width: '100%', marginTop: '25'}}>
            <Header icon='filter' attached color='teal' content='Filters'/>
            <Menu.Item content='All Activities'/>
            <Menu.Item content="I'm going"/>
            <Menu.Item content="I'm hosting"/>
        </Menu>
            <Header/>
            <Calendar/>
        </>
    )
}
```

#### Styling the homepage

- changes in styles.css

```tsx
export default function HomePage() {
    return (
        <Segment inverted textAlign='center' vertical className='masthead'>
            <Container text>
                <Header as='h1' inverted>
                    <Image size='massive' src='assets/logo.png' alt='logo' style={{marginBottom: 12}}/>
                    Reactivities
                </Header>
                <Header as='h2' inverted content='Welcome to reactivities'/>
                <Button as={Link} to='/activities' size='huge' inverted>
                    Take me to the Activities
                </Button>
            </Container>
        </Segment>
    )
}
```

### Error handling

##### Backend Changes

- Created models **AppException** and **Result**

```csharp
namespace Application.Core
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T Value { get; set; }
        public string Error { get; set; }

        public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };
        public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error

    public class AppException
    {
        public AppException(int statusCode, string message, string details = null)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    } };
    }
}
```

- Installed FluentValidation to Core Layer and configured
  
  ```csharp
  namespace Application.Activities
  {
      public class ActivityValidator : AbstractValidator<Activity>
      {
          public ActivityValidator()
          {
              RuleFor(x => x.Title).NotEmpty();
              RuleFor(x => x.Description).NotEmpty();
              RuleFor(x => x.Date).NotEmpty();
              RuleFor(x => x.Category).NotEmpty();
              RuleFor(x => x.City).NotEmpty();
              RuleFor(x => x.Venue).NotEmpty();
          }
      }
  }
  ```

- Made adjustments on ActivityHandlers

```csharp
  namespace Application.Activities
  {
      public class Create
      {
          public class Command : IRequest<Result<Unit>>
          {
              public Activity Activity { get; set; }
          }

          public class CommandValidator : AbstractValidator<Command>
          {
              public CommandValidator()
              {
                  RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
              }
          }

          public class Handler : IRequestHandler<Command, Result<Unit>>
          {
              private readonly DataContext _context;
              public Handler(DataContext context)
              {
                  _context = context;
              }

              public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
              {
                  _context.Activities.Add(request.Activity);

                  var result = await _context.SaveChangesAsync() > 0;

                  if (!result) return Result<Unit>.Failure("Failed to create activity");

                  return Result<Unit>.Success(Unit.Value);
              }
          }
      }
    public class Delete
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await _context.Activities.FindAsync(request.Id);

                // if (activity == null) return null;

                _context.Remove(activity);

                var result = await _context.SaveChangesAsync() > 0;

                if (!result) return Result<Unit>.Failure("Failed to delete the activity");

                return Result<Unit>.Success(Unit.Value);
            }
        }
    }
    public class Details
    {
        public class Query : IRequest<Result<Activity>>
        {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<Activity>>
        {
            private readonly DataContext _context;
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<Activity>> Handle(Query request, CancellationToken cancellationToken)
            {
                var activity = await _context.Activities.FindAsync(request.Id);

                return Result<Activity>.Success(activity);
            }
        }
    }
  public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Activity Activity { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
            }
        }

      public class Handler : IRequestHandler<Command, Result<Unit>>
      {
          private readonly DataContext _context;
          private readonly IMapper _mapper;
          public Handler(DataContext context, IMapper mapper)
          {
              _mapper = mapper;
              _context = context;
          }

          public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
          {
              var activity = await _context.Activities.FindAsync(request.Activity.Id);

              if (activity == null) return null;

              _mapper.Map(request.Activity, activity);

              var result = await _context.SaveChangesAsync() > 0;

              if (!result) return Result<Unit>.Failure("Failed to update activity");

              return Result<Unit>.Success(Unit.Value);
          }
      }
  }
      public class List
  {
      public class Query : IRequest<Result<List<Activity>>> { }

      public class Handler : IRequestHandler<Query, Result<List<Activity>>>
      {
          private readonly DataContext _context;
          public Handler(DataContext context)
          {
              _context = context;
          }

          public async Task<Result<List<Activity>>> Handle(Query request, CancellationToken cancellationToken)
          {
              return Result<List<Activity>>.Success(await _context.Activities.ToListAsync(cancellationToken));
          }
      }
  }
}
```

- Added FluentValidation to startup and Exception Middleware
  
  ```csharp
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddControllers().AddFluentValidation(config =>
      {
          config.RegisterValidatorsFromAssemblyContaining<Create>();
      });
      services.AddApplicationServices(_config);
  }
   public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
      app.UseMiddleware<ExceptionMiddleware>();
      //...
  }
  ```

- Modifications on BaseApiController and ActivitiesController

```csharp
namespace API.Controllers
{
 

    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices
            .GetService<IMediator>();

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null) return NotFound();
            if (result.IsSuccess && result.Value != null)
                return Ok(result.Value);
            if (result.IsSuccess && result.Value == null)
                return NotFound();
            return BadRequest(result.Error);
        }
    }
}
```