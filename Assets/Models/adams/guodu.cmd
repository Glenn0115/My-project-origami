! MODEL_2: one physical actuator at the anchored lower hinge.
! This is a single motor, not 16 imposed hinge motions.
! It moves AUTO_BASE_2_4 smoothly from 0 to +10 degrees in 5 seconds.
! The other seams respond through their physical Revolute/Bushing connections.

constraint create motion_generator motion_name = .MODEL_2.FOLD_SERVO_BASE_2_4 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_BASE_2_4 &
    function = "STEP(time, 0, 0d, 5, 10d)"
