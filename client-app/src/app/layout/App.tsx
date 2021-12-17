import { Container } from 'semantic-ui-react';
import NavBar from "./NavBar";
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard';
import { observer } from "mobx-react-lite";
import HomePage from '../../features/home/HomePage';
import { Route, Routes } from 'react-router-dom';
import ActivityForm from '../../features/activities/form/ActivityForm';

function App() {
  return (
    <>
      <NavBar />
      <Container style={{ marginTop: "5em" }}>
        <Routes>
          <Route path='/' element={<HomePage />} />
          <Route path='/activities' element={<ActivityDashboard />} />
          <Route path='/createActivity' element={<ActivityForm />} />
        </Routes>
      </Container>
    </>
  );
}

export default observer(App);