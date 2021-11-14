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