! MODEL_2 -- 8 driven base hinges / passive side hinges / bottom contact stops.
! Run ONCE after opening the audited latest MODEL_92999.bin.
! This script deliberately does NOT add eight motion generators: that would
! overconstrain the rigid closed loop.  Eight equal torques are the actuators.
!
! Resulting topology:
!   - all 16 AUTO_REV_* side hinges remain passive;
!   - AUTO_BASE_2_4 remains one exact hinge;
!   - the other seven physical base hinges become compliant hinges;
!   - all eight base seams receive the same torque;
!   - PART2 has contact protection against every panel.

! Remove the 16 old SIDE motions.
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_17_18
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_18_15
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_15_16
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_16_13
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_13_14
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_14_11
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_11_12
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_12_9
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_9_10
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_10_7
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_7_8
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_8_5
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_5_6
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_6_3
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_3_4
constraint delete &
    constraint_name = .MODEL_2.FOLD_ALL_4_17

! This Fixed joint duplicates AUTO_BASE_2_4 and must not coexist with it.
constraint delete &
    constraint_name = .MODEL_2.AUTO_FIX_CAP_PANEL4

! Keep AUTO_BASE_2_4 as the reference Revolute.  Convert the other
! seven closed-loop base Revolutes to compliant hinges.
constraint delete &
    constraint_name = .MODEL_2.AUTO_BASE_2_6
constraint delete &
    constraint_name = .MODEL_2.AUTO_BASE_2_8
constraint delete &
    constraint_name = .MODEL_2.AUTO_BASE_2_10
constraint delete &
    constraint_name = .MODEL_2.AUTO_BASE_2_12
constraint delete &
    constraint_name = .MODEL_2.AUTO_BASE_2_14
constraint delete &
    constraint_name = .MODEL_2.AUTO_BASE_2_16
constraint delete &
    constraint_name = .MODEL_2.AUTO_BASE_2_18

! Stiff in translation and transverse rotation; free enough about local Z
! (the fold axis) to prevent a rigid closed loop.
force create element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_6 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_6_I &
    j_marker_name = .MODEL_2.PART6.MK_AUTO_BASE_2_6_J &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002
force create element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_8 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_8_I &
    j_marker_name = .MODEL_2.PART8.MK_AUTO_BASE_2_8_J &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002
force create element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_10 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_10_I &
    j_marker_name = .MODEL_2.PART10.MK_AUTO_BASE_2_10_J &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002
force create element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_12 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_12_I &
    j_marker_name = .MODEL_2.PART12.MK_AUTO_BASE_2_12_J &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002
force create element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_14 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_14_I &
    j_marker_name = .MODEL_2.PART14.MK_AUTO_BASE_2_14_J &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002
force create element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_16 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_16_I &
    j_marker_name = .MODEL_2.PART16.MK_AUTO_BASE_2_16_J &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002
force create element_like bushing &
    bushing_name = .MODEL_2.BUSH_BASE_2_18 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_18_I &
    j_marker_name = .MODEL_2.PART18.MK_AUTO_BASE_2_18_J &
    stiffness = 20, 20, 20 &
    damping = 0.20, 0.20, 0.20 &
    tstiffness = 20, 20, 0.001 &
    tdamping = 0.20, 0.20, 0.002

! Eight equal physical torque actuators.  Positive torque is the outward
! folding sense defined by the generated marker axes.
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_4 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_4_I &
    j_marker_name = .MODEL_2.PART4.MK_AUTO_BASE_2_4_J &
    function = "STEP(time, 0, 0, 2, 0.10)"
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_6 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_6_I &
    j_marker_name = .MODEL_2.PART6.MK_AUTO_BASE_2_6_J &
    function = "STEP(time, 0, 0, 2, 0.10)"
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_8 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_8_I &
    j_marker_name = .MODEL_2.PART8.MK_AUTO_BASE_2_8_J &
    function = "STEP(time, 0, 0, 2, 0.10)"
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_10 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_10_I &
    j_marker_name = .MODEL_2.PART10.MK_AUTO_BASE_2_10_J &
    function = "STEP(time, 0, 0, 2, 0.10)"
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_12 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_12_I &
    j_marker_name = .MODEL_2.PART12.MK_AUTO_BASE_2_12_J &
    function = "STEP(time, 0, 0, 2, 0.10)"
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_14 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_14_I &
    j_marker_name = .MODEL_2.PART14.MK_AUTO_BASE_2_14_J &
    function = "STEP(time, 0, 0, 2, 0.10)"
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_16 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_16_I &
    j_marker_name = .MODEL_2.PART16.MK_AUTO_BASE_2_16_J &
    function = "STEP(time, 0, 0, 2, 0.10)"
force create direct single_component_force &
    single_component_force_name = .MODEL_2.TORQUE_BASE_2_18 &
    type_of_freedom = rotational &
    action_only = off &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_18_I &
    j_marker_name = .MODEL_2.PART18.MK_AUTO_BASE_2_18_J &
    function = "STEP(time, 0, 0, 2, 0.10)"

! Bottom-cap contact stops: Part2.SOLID1 versus every panel solid.
! Friction is deliberately off for a stable first motion test.
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_3 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART3.SOLID2 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_4 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART4.SOLID3 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_5 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART5.SOLID4 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_6 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART6.SOLID5 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_7 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART7.SOLID6 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_8 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART8.SOLID7 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_9 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART9.SOLID8 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
contact create &
    contact_name = .MODEL_2.CONTACT_CAP_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_2.PART2.SOLID1 &
    j_geometry_name = .MODEL_2.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.20 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true
