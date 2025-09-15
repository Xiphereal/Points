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
    Contribution *-- "[10, 20, 40]" Point : of
    Objective *-- "1..*" Contribution : requires a\n minimum of

    class Penalty
    Objective *-- "1..*" Penalty : if failed has
    Penalty *-- "[10, 20, 40]" Point : of
    Person --> "*" Penalty : incurs in
    
    class Reward
    class Goal
    Group --> "1" Reward : sets a
    Group --> "1" Goal : sets a
    Reward --> Goal : is redeemed by achieving a
    Goal *-- "*" Point
```

## Exemplary user stories

- A Group set a Goal of Points for a Reward.
- If a Group achieves a Goal, the set Reward is redeemed.
- A Person can set their own Objectives.
- A Person can perform a Contribution of Points.
- A Person may incur in a Penarly of Points for missing an Objective Contribution.
- A Group can reset the achieved Points. 