import axios, { AxiosResponse } from "axios";
import { Activity } from "../models/activity";

axios.defaults.baseURL = "http://localhost:5000/api";

const responseBody = <T>(response: AxiosResponse<T>) => response.data;

const requests = {
    get: <T>(url: string) => axios.get<T>(url).then(responseBody),
    post: <T>(url: string) => axios.post<T>(url).then(responseBody),
    put: <T>(url: string) => axios.put<T>(url).then(responseBody),
    del: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

const Activities = {
    list: () => requests.get<Activity[]>("/activities"),
};

const agent = {
    Activities,
};

export default agent;