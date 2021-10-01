import React from "react";
import { Card, Image, Button } from 'semantic-ui-react';
import { Activity } from "../../../app/models/activity";

interface Props {
    activity: Activity
    cancelSelectActivity: () => void;

}

export default function ActivityDetails({ activity, cancelSelectActivity }: Props) {
    return (
        <Card fluid>
            <Image src={`/assets/categoryImages/${activity.category}.jpg`} />
            <Card.Content>
                <Card.Header>
                    {activity.title}
                </Card.Header>
                <Card.Meta>
                    <span>{activity.date}</span>
                </Card.Meta>
                <Card.Description>
                    {activity.description}
                </Card.Description>
            </Card.Content>
            <Card.Content>
                <Button.Group widths='2'>
                    <Button basic color='blue' content='edit' />
                    <Button onClick={cancelSelectActivity} basic color='grey' content='cancel' />
                </Button.Group>
            </Card.Content>
        </Card>
    )
}