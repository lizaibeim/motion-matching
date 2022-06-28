enum AnimationCycle {
    Idle = 300,
    Idle_to_walk_ultraSlow = 230,
    Idle_to_walk_verySLow = 220,
    Idle_turn_backL = 100,
    Idle_turn_backR = 100,
    Idle_turn_left = 100,
    Idle_turn_right = 100,
    Run_cycle = 100,
    Run_cycle2 = 100,
    Run_cycle3 = 100,
    Run_to_idle = 70,
    Run_to_walk = 110,
    Run_turn_backL = 80,
    Run_turn_backR = 80,
    Run_turn_left = 100,
    Run_turn_circleL = 100,
    Run_turn_right = 100,
    Run_turn_circleR = 100,
    Run_turn_right_reversed = 100,
    Run_turn_circleR_reversed = 100,
    Walk_circle_left = 140,
    Walk_circle_right = 120,
    Walk_cycle_normal = 120,
    Walk_cycle_slow = 150,
    Walk_cycle_very_slow = 240,
    Walk_slow_down = 130,
    Walk_speed_up = 180,
    Walk_to_idle = 120,
    Walk_to_run = 120,
    Walk_turn_backL = 80,
    Walk_turn_backR = 80,
    Walk_turn_left = 70,
    Walk_turn_right = 70
}

enum AnimationSpeed {
    Idle = 10,
    Idle_to_walk_ultraSlow = 10,
    Idle_to_walk_verySLow = 10,
    Idle_turn_backL = 10,
    Idle_turn_backR = 10,
    Idle_turn_left = 10,
    Idle_turn_right = 10,
    Run_cycle = 10,
    Run_cycle2 = 10,
    Run_cycle3 = 10,
    Run_to_idle = 10,
    Run_to_walk = 10,
    Run_turn_backL = 12,
    Run_turn_backR = 12,
    Run_turn_left = 10,
    Run_turn_circleL = 10,
    Run_turn_right = 10,
    Run_turn_circleR = 10,
    Run_turn_right_reversed = 10,
    Run_turn_circleR_reversed = 10,
    Walk_circle_left = 10,
    Walk_circle_right = 10,
    Walk_cycle_normal = 10,
    Walk_cycle_slow = 10,
    Walk_cycle_very_slow = 10,
    Walk_slow_down = 10,
    Walk_speed_up = 10,
    Walk_to_idle = 10,
    Walk_to_run = 10,
    Walk_turn_backL = 12,
    Walk_turn_backR = 12,
    Walk_turn_left = 10,
    Walk_turn_right = 10

}



public class Cycle {
    public static float GetDiscrepTime(string s)
    {
        switch(s) {
            case "Idle":
                return (float)AnimationCycle.Idle / 100;

            case "Idle_to_walk_ultraSlow":
                return (float)AnimationCycle.Idle_to_walk_ultraSlow / 100;

            case "Idle_to_walk_verySlow":
                return (float)AnimationCycle.Idle_to_walk_verySLow / 100;

            case "Idle_turn_backL":
                return (float)AnimationCycle.Idle_turn_backL / 100;
            
            case "Idle_turn_backR":
                return (float)AnimationCycle.Idle_turn_backR / 100;
            
            case "Idle_turn_left":
                return (float)AnimationCycle.Idle_turn_left / 100;
            
            case "Idle_turn_right":
                return (float)AnimationCycle.Idle_turn_right / 100;
            
            case "Run_cycle":
                return (float)AnimationCycle.Run_cycle / 100;
            
            case "Run_cycle2":
                return (float)AnimationCycle.Run_cycle2 / 100;
            
            case "Run_cycle3":
                return (float)AnimationCycle.Run_cycle3 / 100;
            
            case "Run_to_idle":
                return (float)AnimationCycle.Run_to_idle / 100;
            
            case "Run_to_walk":
                return (float)AnimationCycle.Run_to_walk / 100;
            
            case "Run_turn_backL":
                return (float)AnimationCycle.Run_turn_backL / 100;
            
            case "Run_turn_backR":
                return (float)AnimationCycle.Run_turn_backR / 100;
            
            case "Run_turn_left":
                return (float)AnimationCycle.Run_turn_left / 100;
            
            case "Run_turn_circleL":
                return (float)AnimationCycle.Run_turn_circleL / 100;
            
            case "Run_turn_right":
                return (float)AnimationCycle.Run_turn_right / 100;
            
            case "Run_turn_circleR":
                return (float)AnimationCycle.Run_turn_circleR / 100;
            
            case "Walk_circle_left":
                return (float)AnimationCycle.Walk_circle_left / 100;
            
            case "Walk_circle_right":
                return (float)AnimationCycle.Walk_circle_right / 100;
            
            case "Walk_cycle_normal":
                return (float)AnimationCycle.Walk_cycle_normal / 100;
            
            case "Walk_cycle_slow":
                return (float)AnimationCycle.Walk_cycle_slow / 100;
            
            case "Walk_cycle_very_slow":
                return (float)AnimationCycle.Walk_cycle_very_slow / 100;
            
            case "Walk_slow_down":
                return (float)AnimationCycle.Walk_slow_down / 100;
            
            case "Walk_speed_up":
                return (float)AnimationCycle.Walk_speed_up / 100;
            
            case "Walk_to_idle":
                return (float)AnimationCycle.Walk_to_idle / 100;
            
            case "Walk_to_run":
                return (float)AnimationCycle.Walk_to_run / 100;
            
            case "Walk_turn_backL":
                return (float)AnimationCycle.Walk_turn_backL / 100;
            
            case "Walk_turn_backR":
                return (float)AnimationCycle.Walk_turn_backR / 100;
            
            case "Walk_turn_left":
                return (float)AnimationCycle.Walk_turn_left / 100;
            
            case "Walk_turn_right":
                return (float)AnimationCycle.Walk_turn_right / 100;

            default:
                return 1f;
        }
    }


     public static float GetSpeed(string s)
    {
        switch(s) {
            case "Idle":
                return (float)AnimationSpeed.Idle / 10;

            case "Idle_to_walk_ultraSlow":
                return (float)AnimationSpeed.Idle_to_walk_ultraSlow / 10;

            case "Idle_to_walk_verySlow":
                return (float)AnimationSpeed.Idle_to_walk_verySLow / 10;

            case "Idle_turn_backL":
                return (float)AnimationSpeed.Idle_turn_backL / 10;
            
            case "Idle_turn_backR":
                return (float)AnimationSpeed.Idle_turn_backR / 10;
            
            case "Idle_turn_left":
                return (float)AnimationSpeed.Idle_turn_left / 10;
            
            case "Idle_turn_right":
                return (float)AnimationSpeed.Idle_turn_right / 10;
            
            case "Run_cycle":
                return (float)AnimationSpeed.Run_cycle / 10;
            
            case "Run_cycle2":
                return (float)AnimationSpeed.Run_cycle2 / 10;
            
            case "Run_cycle3":
                return (float)AnimationSpeed.Run_cycle3 / 10;
            
            case "Run_to_idle":
                return (float)AnimationSpeed.Run_to_idle / 10;
            
            case "Run_to_walk":
                return (float)AnimationSpeed.Run_to_walk / 10;
            
            case "Run_turn_backL":
                return (float)AnimationSpeed.Run_turn_backL / 10;
            
            case "Run_turn_backR":
                return (float)AnimationSpeed.Run_turn_backR / 10;
            
            case "Run_turn_left":
                return (float)AnimationSpeed.Run_turn_left / 10;
            
            case "Run_turn_circleL":
                return (float)AnimationSpeed.Run_turn_circleL / 10;
            
            case "Run_turn_right":
                return (float)AnimationSpeed.Run_turn_right / 10;
            
            case "Run_turn_circleR":
                return (float)AnimationSpeed.Run_turn_circleR / 10;
            
            case "Walk_circle_left":
                return (float)AnimationSpeed.Walk_circle_left / 10;
            
            case "Walk_circle_right":
                return (float)AnimationSpeed.Walk_circle_right / 10;
            
            case "Walk_cycle_normal":
                return (float)AnimationSpeed.Walk_cycle_normal / 10;
            
            case "Walk_cycle_slow":
                return (float)AnimationSpeed.Walk_cycle_slow / 10;
            
            case "Walk_cycle_very_slow":
                return (float)AnimationSpeed.Walk_cycle_very_slow / 10;
            
            case "Walk_slow_down":
                return (float)AnimationSpeed.Walk_slow_down / 10;
            
            case "Walk_speed_up":
                return (float)AnimationSpeed.Walk_speed_up / 10;
            
            case "Walk_to_idle":
                return (float)AnimationSpeed.Walk_to_idle / 10;
            
            case "Walk_to_run":
                return (float)AnimationSpeed.Walk_to_run / 10;
            
            case "Walk_turn_backL":
                return (float)AnimationSpeed.Walk_turn_backL / 10;
            
            case "Walk_turn_backR":
                return (float)AnimationSpeed.Walk_turn_backR / 10;
            
            case "Walk_turn_left":
                return (float)AnimationSpeed.Walk_turn_left / 10;
            
            case "Walk_turn_right":
                return (float)AnimationSpeed.Walk_turn_right / 10;

            default:
                return 1f;
        }
    }

}
