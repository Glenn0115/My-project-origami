! MODEL_2: replace the cap-to-PART4 fixed joint by eight bottom-edge hinges.
! PART2 remains fixed to GROUND through AUTO_FIX_CAP_GROUND.
! The side panels connected to the cap are PART4, 6, 8, 10, 12, 14, 16, 18.

constraint delete joint joint_name = .MODEL_2.AUTO_FIX_CAP_PANEL4

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_4_I &
    location = -10.6195687531, 33, 9.02929079522 &
    orientation = 90, 119.538181751, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART4.MK_AUTO_BASE_2_4_J &
    location = -10.6195687531, 33, 9.02929079522 &
    orientation = 90, 119.538181751, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_4 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_4_I &
    j_marker_name = .MODEL_2.PART4.MK_AUTO_BASE_2_4_J

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_6_I &
    location = -8.34632173496, 33, 6.11566723618 &
    orientation = 90, 164.538181751, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART6.MK_AUTO_BASE_2_6_J &
    location = -8.34632173496, 33, 6.11566723618 &
    orientation = 90, 164.538181751, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_6 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_6_I &
    j_marker_name = .MODEL_2.PART6.MK_AUTO_BASE_2_6_J

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_8_I &
    location = -8.79913632952, 33, 2.44799587788 &
    orientation = -90, 150.461818249, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART8.MK_AUTO_BASE_2_8_J &
    location = -8.79913632952, 33, 2.44799587788 &
    orientation = -90, 150.461818249, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_8 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_8_I &
    j_marker_name = .MODEL_2.PART8.MK_AUTO_BASE_2_8_J

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_10_I &
    location = -11.7127598886, 33, 0.174748859706 &
    orientation = -90, 105.461818249, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART10.MK_AUTO_BASE_2_10_J &
    location = -11.7127598886, 33, 0.174748859706 &
    orientation = -90, 105.461818249, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_10 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_10_I &
    j_marker_name = .MODEL_2.PART10.MK_AUTO_BASE_2_10_J

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_12_I &
    location = -15.3804312469, 33, 0.627563454268 &
    orientation = -90, 60.4618182495, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART12.MK_AUTO_BASE_2_12_J &
    location = -15.3804312469, 33, 0.627563454268 &
    orientation = -90, 60.4618182495, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_12 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_12_I &
    j_marker_name = .MODEL_2.PART12.MK_AUTO_BASE_2_12_J

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_14_I &
    location = -17.653678265, 33, 3.54118701332 &
    orientation = -90, 15.4618182495, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART14.MK_AUTO_BASE_2_14_J &
    location = -17.653678265, 33, 3.54118701332 &
    orientation = -90, 15.4618182495, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_14 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_14_I &
    j_marker_name = .MODEL_2.PART14.MK_AUTO_BASE_2_14_J

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_16_I &
    location = -17.2008636705, 33, 7.20885837161 &
    orientation = 90, 29.5381817505, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART16.MK_AUTO_BASE_2_16_J &
    location = -17.2008636705, 33, 7.20885837161 &
    orientation = 90, 29.5381817505, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_16 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_16_I &
    j_marker_name = .MODEL_2.PART16.MK_AUTO_BASE_2_16_J

marker create marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_18_I &
    location = -14.2872401114, 33, 9.48210538979 &
    orientation = 90, 74.5381817505, 0 &
    relative_to = .MODEL_2.GROUND
marker create marker_name = .MODEL_2.PART18.MK_AUTO_BASE_2_18_J &
    location = -14.2872401114, 33, 9.48210538979 &
    orientation = 90, 74.5381817505, 0 &
    relative_to = .MODEL_2.GROUND
constraint create joint revolute joint_name = .MODEL_2.AUTO_BASE_2_18 &
    i_marker_name = .MODEL_2.PART2.MK_AUTO_BASE_2_18_I &
    j_marker_name = .MODEL_2.PART18.MK_AUTO_BASE_2_18_J
