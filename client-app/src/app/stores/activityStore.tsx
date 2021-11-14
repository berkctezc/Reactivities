import { makeObservable, observable } from "mobx";

export default class ActivitStore {
    title = 'Hello from MobX!';

    constructor() {
        makeObservable(this, {
            title: observable
        })
    }
}