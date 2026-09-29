! MODEL_2: experimental synchronized fold command.
! All 16 revolute joints are commanded with the same angular displacement.
! 0--5 s: 0 to +3 degrees; afterwards the position is held.
! This is intentionally only 3 degrees: this closed mechanism has 10 DOF,
! and identical commands can be geometrically incompatible at larger angles.
!
! If FOLD_DRIVE_MAIN was added earlier, delete it in Adams first:
!   right-click FOLD_DRIVE_MAIN in the model tree -> Delete

constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_17_18 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_17_18 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_18_15 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_18_15 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_15_16 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_15_16 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_16_13 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_16_13 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_13_14 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_13_14 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_14_11 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_14_11 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_11_12 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_11_12 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_12_9 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_12_9 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_9_10 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_9_10 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_10_7 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_10_7 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_7_8 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_7_8 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_8_5 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_8_5 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_5_6 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_5_6 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_6_3 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_6_3 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_3_4 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_3_4 &
    function = "STEP(time, 0, 0d, 5, 3d)"
constraint create motion_generator motion_name = .MODEL_2.FOLD_ALL_4_17 &
    type_of_freedom = rotational &
    joint_name = .MODEL_2.AUTO_REV_4_17 &
    function = "STEP(time, 0, 0d, 5, 3d)"
