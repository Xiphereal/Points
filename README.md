# Points

A simple web app for keeping track of the points achieved on a shared purse for a reward system.

## Domain model

```mermaid
classDiagram
    class Group
    class Person
    Group *-- "*" Person

    class Objective
    class Contribution
    Person --> "*" Objective : has
    Person --> "*" Contribution : performs

    class Point
    Contribution --> Objective : works towards an
    Contribution *-- "[10, 20, 40]" Point : of
    
    class Reward
    class Goal
    Group --> "1" Reward : set a
    Group --> "1" Goal : set a
    Reward --> Goal : is redeemed by achieving a
    Goal *-- "*" Point
```

## Exemplary user stories

- A Group set a Goal of Points for a Reward.
- If a Group achieves a Goal, the set Reward is redeemed.
- A Person can set their own Objectives.
- A Person can perform a Contribution of Points.
- A Group can reset the achieved Points. 