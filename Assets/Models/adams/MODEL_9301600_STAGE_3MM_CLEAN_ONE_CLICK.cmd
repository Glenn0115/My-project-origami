! MODEL_9301600 staged 3 mm / -46.5 deg contact trial; import ONCE into a blank database.
! Requires the clean-source BIN on the Adams computer at the D: path below.
! Replays the proven 0-2 mm / -31 deg in 20 s, holds 5 s, then advances in four 0.25 mm steps.
! Each new step lasts 5 s, with 2 s holds, followed by a final 5 s hold; no BIN save or replay.
! The twist ratio beyond 2 mm is an UNVERIFIED extrapolation; the 3 mm bound is exploratory.
! Contact/bushing forces are measured but do NOT halt the prescribed motion without validated limits.
! Only the 0.20 mm reference cap-gap sensor can halt the run early; it is not a strength limit.
! Stop and inspect if force spikes, contact penetrates, or the mechanism behaves abnormally.
file binary read &
    file_name = "D:\0work\Develop\unity\Origami_Simulator\Assets\Models\adams\MODEL_9301600_CLEAN_SOURCE.bin" &
    alert = yes

simulation single_run reset

constraint delete &
    constraint_name = .MODEL_930.K_TOP_PRESS
constraint delete &
    constraint_name = .MODEL_930.K_TOP_TWIST

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_03 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART3.SOLID2 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_03 &
    object = .MODEL_930.LIMIT_CONTACT_02_03 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_04 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART4.SOLID3 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_04 &
    object = .MODEL_930.LIMIT_CONTACT_02_04 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_05 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART5.SOLID4 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_05 &
    object = .MODEL_930.LIMIT_CONTACT_02_05 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_06 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART6.SOLID5 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_06 &
    object = .MODEL_930.LIMIT_CONTACT_02_06 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_07 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART7.SOLID6 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_07 &
    object = .MODEL_930.LIMIT_CONTACT_02_07 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_08 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART8.SOLID7 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_08 &
    object = .MODEL_930.LIMIT_CONTACT_02_08 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_09 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART9.SOLID8 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_09 &
    object = .MODEL_930.LIMIT_CONTACT_02_09 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_10 &
    object = .MODEL_930.LIMIT_CONTACT_02_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_11 &
    object = .MODEL_930.LIMIT_CONTACT_02_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_12 &
    object = .MODEL_930.LIMIT_CONTACT_02_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_13 &
    object = .MODEL_930.LIMIT_CONTACT_02_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_14 &
    object = .MODEL_930.LIMIT_CONTACT_02_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_15 &
    object = .MODEL_930.LIMIT_CONTACT_02_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_16 &
    object = .MODEL_930.LIMIT_CONTACT_02_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_17 &
    object = .MODEL_930.LIMIT_CONTACT_02_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_18 &
    object = .MODEL_930.LIMIT_CONTACT_02_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_02_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART2.SOLID1 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_02_19 &
    object = .MODEL_930.LIMIT_CONTACT_02_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_04 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART4.SOLID3 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_04 &
    object = .MODEL_930.LIMIT_CONTACT_03_04 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_05 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART5.SOLID4 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_05 &
    object = .MODEL_930.LIMIT_CONTACT_03_05 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_06 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART6.SOLID5 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_06 &
    object = .MODEL_930.LIMIT_CONTACT_03_06 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_07 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART7.SOLID6 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_07 &
    object = .MODEL_930.LIMIT_CONTACT_03_07 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_08 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART8.SOLID7 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_08 &
    object = .MODEL_930.LIMIT_CONTACT_03_08 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_09 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART9.SOLID8 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_09 &
    object = .MODEL_930.LIMIT_CONTACT_03_09 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_10 &
    object = .MODEL_930.LIMIT_CONTACT_03_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_11 &
    object = .MODEL_930.LIMIT_CONTACT_03_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_12 &
    object = .MODEL_930.LIMIT_CONTACT_03_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_13 &
    object = .MODEL_930.LIMIT_CONTACT_03_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_14 &
    object = .MODEL_930.LIMIT_CONTACT_03_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_15 &
    object = .MODEL_930.LIMIT_CONTACT_03_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_16 &
    object = .MODEL_930.LIMIT_CONTACT_03_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_17 &
    object = .MODEL_930.LIMIT_CONTACT_03_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_18 &
    object = .MODEL_930.LIMIT_CONTACT_03_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_03_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART3.SOLID2 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_03_19 &
    object = .MODEL_930.LIMIT_CONTACT_03_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_06 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART6.SOLID5 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_06 &
    object = .MODEL_930.LIMIT_CONTACT_04_06 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_08 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART8.SOLID7 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_08 &
    object = .MODEL_930.LIMIT_CONTACT_04_08 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_09 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART9.SOLID8 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_09 &
    object = .MODEL_930.LIMIT_CONTACT_04_09 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_10 &
    object = .MODEL_930.LIMIT_CONTACT_04_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_11 &
    object = .MODEL_930.LIMIT_CONTACT_04_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_12 &
    object = .MODEL_930.LIMIT_CONTACT_04_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_13 &
    object = .MODEL_930.LIMIT_CONTACT_04_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_14 &
    object = .MODEL_930.LIMIT_CONTACT_04_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_15 &
    object = .MODEL_930.LIMIT_CONTACT_04_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_16 &
    object = .MODEL_930.LIMIT_CONTACT_04_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_17 &
    object = .MODEL_930.LIMIT_CONTACT_04_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_18 &
    object = .MODEL_930.LIMIT_CONTACT_04_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_04_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART4.SOLID3 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_04_19 &
    object = .MODEL_930.LIMIT_CONTACT_04_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_06 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART6.SOLID5 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_06 &
    object = .MODEL_930.LIMIT_CONTACT_05_06 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_07 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART7.SOLID6 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_07 &
    object = .MODEL_930.LIMIT_CONTACT_05_07 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_08 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART8.SOLID7 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_08 &
    object = .MODEL_930.LIMIT_CONTACT_05_08 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_09 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART9.SOLID8 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_09 &
    object = .MODEL_930.LIMIT_CONTACT_05_09 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_10 &
    object = .MODEL_930.LIMIT_CONTACT_05_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_11 &
    object = .MODEL_930.LIMIT_CONTACT_05_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_12 &
    object = .MODEL_930.LIMIT_CONTACT_05_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_13 &
    object = .MODEL_930.LIMIT_CONTACT_05_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_14 &
    object = .MODEL_930.LIMIT_CONTACT_05_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_15 &
    object = .MODEL_930.LIMIT_CONTACT_05_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_16 &
    object = .MODEL_930.LIMIT_CONTACT_05_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_17 &
    object = .MODEL_930.LIMIT_CONTACT_05_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_05_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART5.SOLID4 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_05_19 &
    object = .MODEL_930.LIMIT_CONTACT_05_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_08 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART8.SOLID7 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_08 &
    object = .MODEL_930.LIMIT_CONTACT_06_08 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_10 &
    object = .MODEL_930.LIMIT_CONTACT_06_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_11 &
    object = .MODEL_930.LIMIT_CONTACT_06_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_12 &
    object = .MODEL_930.LIMIT_CONTACT_06_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_13 &
    object = .MODEL_930.LIMIT_CONTACT_06_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_14 &
    object = .MODEL_930.LIMIT_CONTACT_06_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_15 &
    object = .MODEL_930.LIMIT_CONTACT_06_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_16 &
    object = .MODEL_930.LIMIT_CONTACT_06_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_17 &
    object = .MODEL_930.LIMIT_CONTACT_06_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_18 &
    object = .MODEL_930.LIMIT_CONTACT_06_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_06_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART6.SOLID5 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_06_19 &
    object = .MODEL_930.LIMIT_CONTACT_06_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_08 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART8.SOLID7 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_08 &
    object = .MODEL_930.LIMIT_CONTACT_07_08 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_09 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART9.SOLID8 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_09 &
    object = .MODEL_930.LIMIT_CONTACT_07_09 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_10 &
    object = .MODEL_930.LIMIT_CONTACT_07_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_11 &
    object = .MODEL_930.LIMIT_CONTACT_07_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_12 &
    object = .MODEL_930.LIMIT_CONTACT_07_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_13 &
    object = .MODEL_930.LIMIT_CONTACT_07_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_14 &
    object = .MODEL_930.LIMIT_CONTACT_07_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_15 &
    object = .MODEL_930.LIMIT_CONTACT_07_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_16 &
    object = .MODEL_930.LIMIT_CONTACT_07_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_17 &
    object = .MODEL_930.LIMIT_CONTACT_07_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_18 &
    object = .MODEL_930.LIMIT_CONTACT_07_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_07_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART7.SOLID6 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_07_19 &
    object = .MODEL_930.LIMIT_CONTACT_07_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_10 &
    object = .MODEL_930.LIMIT_CONTACT_08_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_12 &
    object = .MODEL_930.LIMIT_CONTACT_08_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_13 &
    object = .MODEL_930.LIMIT_CONTACT_08_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_14 &
    object = .MODEL_930.LIMIT_CONTACT_08_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_15 &
    object = .MODEL_930.LIMIT_CONTACT_08_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_16 &
    object = .MODEL_930.LIMIT_CONTACT_08_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_17 &
    object = .MODEL_930.LIMIT_CONTACT_08_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_18 &
    object = .MODEL_930.LIMIT_CONTACT_08_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_08_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART8.SOLID7 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_08_19 &
    object = .MODEL_930.LIMIT_CONTACT_08_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_10 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART10.SOLID9 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_10 &
    object = .MODEL_930.LIMIT_CONTACT_09_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_11 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART11.SOLID10 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_11 &
    object = .MODEL_930.LIMIT_CONTACT_09_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_12 &
    object = .MODEL_930.LIMIT_CONTACT_09_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_13 &
    object = .MODEL_930.LIMIT_CONTACT_09_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_14 &
    object = .MODEL_930.LIMIT_CONTACT_09_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_15 &
    object = .MODEL_930.LIMIT_CONTACT_09_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_16 &
    object = .MODEL_930.LIMIT_CONTACT_09_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_17 &
    object = .MODEL_930.LIMIT_CONTACT_09_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_18 &
    object = .MODEL_930.LIMIT_CONTACT_09_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_09_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART9.SOLID8 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_09_19 &
    object = .MODEL_930.LIMIT_CONTACT_09_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_10_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART10.SOLID9 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_10_12 &
    object = .MODEL_930.LIMIT_CONTACT_10_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_10_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART10.SOLID9 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_10_14 &
    object = .MODEL_930.LIMIT_CONTACT_10_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_10_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART10.SOLID9 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_10_15 &
    object = .MODEL_930.LIMIT_CONTACT_10_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_10_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART10.SOLID9 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_10_16 &
    object = .MODEL_930.LIMIT_CONTACT_10_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_10_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART10.SOLID9 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_10_17 &
    object = .MODEL_930.LIMIT_CONTACT_10_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_10_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART10.SOLID9 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_10_18 &
    object = .MODEL_930.LIMIT_CONTACT_10_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_10_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART10.SOLID9 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_10_19 &
    object = .MODEL_930.LIMIT_CONTACT_10_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_12 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART12.SOLID11 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_12 &
    object = .MODEL_930.LIMIT_CONTACT_11_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_13 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART13.SOLID12 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_13 &
    object = .MODEL_930.LIMIT_CONTACT_11_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_14 &
    object = .MODEL_930.LIMIT_CONTACT_11_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_15 &
    object = .MODEL_930.LIMIT_CONTACT_11_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_16 &
    object = .MODEL_930.LIMIT_CONTACT_11_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_17 &
    object = .MODEL_930.LIMIT_CONTACT_11_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_18 &
    object = .MODEL_930.LIMIT_CONTACT_11_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_11_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART11.SOLID10 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_11_19 &
    object = .MODEL_930.LIMIT_CONTACT_11_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_12_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART12.SOLID11 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_12_14 &
    object = .MODEL_930.LIMIT_CONTACT_12_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_12_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART12.SOLID11 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_12_16 &
    object = .MODEL_930.LIMIT_CONTACT_12_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_12_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART12.SOLID11 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_12_17 &
    object = .MODEL_930.LIMIT_CONTACT_12_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_12_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART12.SOLID11 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_12_18 &
    object = .MODEL_930.LIMIT_CONTACT_12_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_12_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART12.SOLID11 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_12_19 &
    object = .MODEL_930.LIMIT_CONTACT_12_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_13_14 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART13.SOLID12 &
    j_geometry_name = .MODEL_930.PART14.SOLID13 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_13_14 &
    object = .MODEL_930.LIMIT_CONTACT_13_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_13_15 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART13.SOLID12 &
    j_geometry_name = .MODEL_930.PART15.SOLID14 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_13_15 &
    object = .MODEL_930.LIMIT_CONTACT_13_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_13_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART13.SOLID12 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_13_16 &
    object = .MODEL_930.LIMIT_CONTACT_13_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_13_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART13.SOLID12 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_13_17 &
    object = .MODEL_930.LIMIT_CONTACT_13_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_13_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART13.SOLID12 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_13_18 &
    object = .MODEL_930.LIMIT_CONTACT_13_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_13_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART13.SOLID12 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_13_19 &
    object = .MODEL_930.LIMIT_CONTACT_13_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_14_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART14.SOLID13 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_14_16 &
    object = .MODEL_930.LIMIT_CONTACT_14_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_14_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART14.SOLID13 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_14_18 &
    object = .MODEL_930.LIMIT_CONTACT_14_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_14_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART14.SOLID13 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_14_19 &
    object = .MODEL_930.LIMIT_CONTACT_14_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_15_16 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART15.SOLID14 &
    j_geometry_name = .MODEL_930.PART16.SOLID15 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_15_16 &
    object = .MODEL_930.LIMIT_CONTACT_15_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_15_17 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART15.SOLID14 &
    j_geometry_name = .MODEL_930.PART17.SOLID16 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_15_17 &
    object = .MODEL_930.LIMIT_CONTACT_15_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_15_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART15.SOLID14 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_15_18 &
    object = .MODEL_930.LIMIT_CONTACT_15_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_15_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART15.SOLID14 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_15_19 &
    object = .MODEL_930.LIMIT_CONTACT_15_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_16_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART16.SOLID15 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_16_18 &
    object = .MODEL_930.LIMIT_CONTACT_16_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_17_18 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART17.SOLID16 &
    j_geometry_name = .MODEL_930.PART18.SOLID17 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_17_18 &
    object = .MODEL_930.LIMIT_CONTACT_17_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

contact create &
    contact_name = .MODEL_930.LIMIT_CONTACT_17_19 &
    type = solid_to_solid &
    i_geometry_name = .MODEL_930.PART17.SOLID16 &
    j_geometry_name = .MODEL_930.PART19.SOLID18 &
    stiffness = 100 &
    damping = 0.2 &
    exponent = 2.2 &
    dmax = 0.10 &
    no_friction = true

measure create object &
    measure_name = .MODEL_930.LIMIT_FORCE_17_19 &
    object = .MODEL_930.LIMIT_CONTACT_17_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no

measure create function &
    measure_name = .MODEL_930.LIMIT_STROKE &
    function = "8-DY(.MODEL_930.PART2.MK_K_TOP_GUIDE_I,.MODEL_930.PART3.MK_K_BOTTOM_FIXED_I)" &
    create_measure_display = yes

measure create function &
    measure_name = .MODEL_930.LIMIT_CAP_GAP &
    function = "DY(.MODEL_930.PART2.MK_K_TOP_2_4_I,.MODEL_930.PART3.MK_K_BOTTOM_3_5_I)" &
    create_measure_display = yes

measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_5 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_5_I,.MODEL_930.PART5.MK_K_BOTTOM_3_5_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_5 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_5 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_7 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_7_I,.MODEL_930.PART7.MK_K_BOTTOM_3_7_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_7 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_7 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_9 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_9_I,.MODEL_930.PART9.MK_K_BOTTOM_3_9_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_9 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_9 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_11 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_11_I,.MODEL_930.PART11.MK_K_BOTTOM_3_11_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_11 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_11 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_13 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_13_I,.MODEL_930.PART13.MK_K_BOTTOM_3_13_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_13 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_13 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_15 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_15_I,.MODEL_930.PART15.MK_K_BOTTOM_3_15_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_15 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_15 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_17 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_17_I,.MODEL_930.PART17.MK_K_BOTTOM_3_17_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_17 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_17 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_BOTTOM_3_19 &
    function = "DM(.MODEL_930.PART3.MK_K_BOTTOM_3_19_I,.MODEL_930.PART19.MK_K_BOTTOM_3_19_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_BOTTOM_3_19 &
    object = .MODEL_930.K_BOTTOM_BUSH_3_19 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_4 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_4_I,.MODEL_930.PART4.MK_K_TOP_2_4_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_4 &
    object = .MODEL_930.K_TOP_BUSH_2_4 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_6 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_6_I,.MODEL_930.PART6.MK_K_TOP_2_6_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_6 &
    object = .MODEL_930.K_TOP_BUSH_2_6 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_8 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_8_I,.MODEL_930.PART8.MK_K_TOP_2_8_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_8 &
    object = .MODEL_930.K_TOP_BUSH_2_8 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_10 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_10_I,.MODEL_930.PART10.MK_K_TOP_2_10_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_10 &
    object = .MODEL_930.K_TOP_BUSH_2_10 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_12 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_12_I,.MODEL_930.PART12.MK_K_TOP_2_12_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_12 &
    object = .MODEL_930.K_TOP_BUSH_2_12 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_14 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_14_I,.MODEL_930.PART14.MK_K_TOP_2_14_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_14 &
    object = .MODEL_930.K_TOP_BUSH_2_14 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_16 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_16_I,.MODEL_930.PART16.MK_K_TOP_2_16_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_16 &
    object = .MODEL_930.K_TOP_BUSH_2_16 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


measure create function &
    measure_name = .MODEL_930.LIMIT_BUSH_GAP_TOP_2_18 &
    function = "DM(.MODEL_930.PART2.MK_K_TOP_2_18_I,.MODEL_930.PART18.MK_K_TOP_2_18_J)" &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_BUSH_FORCE_TOP_2_18 &
    object = .MODEL_930.K_TOP_BUSH_2_18 &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = no


executive_control create sensor &
    sensor_name = .MODEL_930.LIMIT_STOP_CAP_GAP &
    function = "DY(.MODEL_930.PART2.MK_K_TOP_2_4_I,.MODEL_930.PART3.MK_K_BOTTOM_3_5_I)" &
    compare = le &
    value = 0.2 &
    error = 0.0001 &
    bisection = on &
    time_error = 0.001 &
    halt = on &
    print = on

constraint create motion_generator &
    motion_name = .MODEL_930.LIMIT_TOP_PRESS &
    type_of_freedom = translational &
    joint_name = .MODEL_930.K_TOP_GUIDE &
    time_derivative = displacement &
    function = "STEP(time,0,0,20,-2)+STEP(time,25,0,30,-0.25)+STEP(time,32,0,37,-0.25)+STEP(time,39,0,44,-0.25)+STEP(time,46,0,51,-0.25)"

measure create object &
    measure_name = .MODEL_930.LIMIT_PRESS_FORCE &
    object = .MODEL_930.LIMIT_TOP_PRESS &
    characteristic = element_force &
    component = mag_component &
    create_measure_display = yes

constraint create motion_generator &
    motion_name = .MODEL_930.LIMIT_TOP_TWIST &
    type_of_freedom = rotational &
    joint_name = .MODEL_930.K_TOP_GUIDE &
    time_derivative = displacement &
    function = "STEP(time,0,0d,20,-31d)+STEP(time,25,0d,30,-3.875d)+STEP(time,32,0d,37,-3.875d)+STEP(time,39,0d,44,-3.875d)+STEP(time,46,0d,51,-3.875d)"

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART2 &
    object = .MODEL_930.PART2 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART2 &
    object = .MODEL_930.PART2 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART4 &
    object = .MODEL_930.PART4 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART4 &
    object = .MODEL_930.PART4 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART5 &
    object = .MODEL_930.PART5 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART5 &
    object = .MODEL_930.PART5 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART6 &
    object = .MODEL_930.PART6 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART6 &
    object = .MODEL_930.PART6 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART7 &
    object = .MODEL_930.PART7 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART7 &
    object = .MODEL_930.PART7 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART8 &
    object = .MODEL_930.PART8 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART8 &
    object = .MODEL_930.PART8 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART9 &
    object = .MODEL_930.PART9 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART9 &
    object = .MODEL_930.PART9 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART10 &
    object = .MODEL_930.PART10 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART10 &
    object = .MODEL_930.PART10 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART11 &
    object = .MODEL_930.PART11 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART11 &
    object = .MODEL_930.PART11 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART12 &
    object = .MODEL_930.PART12 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART12 &
    object = .MODEL_930.PART12 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART13 &
    object = .MODEL_930.PART13 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART13 &
    object = .MODEL_930.PART13 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART14 &
    object = .MODEL_930.PART14 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART14 &
    object = .MODEL_930.PART14 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART15 &
    object = .MODEL_930.PART15 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART15 &
    object = .MODEL_930.PART15 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART16 &
    object = .MODEL_930.PART16 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART16 &
    object = .MODEL_930.PART16 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART17 &
    object = .MODEL_930.PART17 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART17 &
    object = .MODEL_930.PART17 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART18 &
    object = .MODEL_930.PART18 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART18 &
    object = .MODEL_930.PART18 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_SPEED_PART19 &
    object = .MODEL_930.PART19 &
    characteristic = cm_velocity &
    component = mag_component &
    create_measure_display = no

measure create object &
    measure_name = .MODEL_930.LIMIT_ANGULAR_SPEED_PART19 &
    object = .MODEL_930.PART19 &
    characteristic = cm_angular_velocity &
    component = mag_component &
    create_measure_display = no

simulation single_run transient &
    model_name = .MODEL_930 &
    type = dynamic &
    initial_static = yes &
    end_time = 56 &
    number_of_steps = 2800
