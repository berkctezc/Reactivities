export interface Activity {
    map(arg0: (activity: any) => JSX.Element): import("react").ReactNode;
    id: string;
    title: string;
    date: Date;
    description: string;
    category: string;
    city: string;
    venue: string;
}