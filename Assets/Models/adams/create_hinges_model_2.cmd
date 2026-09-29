! Generated for MODEL_2 from 9299.stp.
! Expected geometry: exactly PART2 through PART18 (17 bodies).
! PART2 is the bottom cap.  It is grounded and fixed to PART4.
! Do NOT import the STEP a second time into the same model.

marker create marker_name = .MODEL_2.PART17.MK_REV_17_18_I &
    location = -15.0940252919, 36.5, 8.42186241336 &
    orientation = -142.000994786, 109.735973049, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART18.MK_REV_17_18_J &
    location = -15.0940252919, 36.5, 8.42186241336 &
    orientation = -142.000994786, 109.735973049, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_17_18 &
    i_marker_name = .MODEL_2.PART17.MK_REV_17_18_I &
    j_marker_name = .MODEL_2.PART18.MK_REV_17_18_J

marker create marker_name = .MODEL_2.PART18.MK_REV_18_15_I &
    location = -17.0216419442, 36.5, 7.88867010117 &
    orientation = -167.019483046, 106.445848399, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART15.MK_REV_18_15_J &
    location = -17.0216419442, 36.5, 7.88867010117 &
    orientation = -167.019483046, 106.445848399, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_18_15 &
    i_marker_name = .MODEL_2.PART18.MK_REV_18_15_I &
    j_marker_name = .MODEL_2.PART15.MK_REV_18_15_J

marker create marker_name = .MODEL_2.PART15.MK_REV_15_16_I &
    location = -17.0216419442, 36.5, 5.88867010117 &
    orientation = -167.019483046, 130.431405699, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART16.MK_REV_15_16_J &
    location = -17.0216419442, 36.5, 5.88867010117 &
    orientation = -167.019483046, 130.431405699, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_15_16 &
    i_marker_name = .MODEL_2.PART15.MK_REV_15_16_I &
    j_marker_name = .MODEL_2.PART16.MK_REV_15_16_J

marker create marker_name = .MODEL_2.PART16.MK_REV_16_13_I &
    location = -18.007648851, 36.5, 4.14861539518 &
    orientation = 177.068660701, 110.641547258, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART13.MK_REV_16_13_J &
    location = -18.007648851, 36.5, 4.14861539518 &
    orientation = 177.068660701, 110.641547258, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_16_13 &
    i_marker_name = .MODEL_2.PART16.MK_REV_16_13_I &
    j_marker_name = .MODEL_2.PART13.MK_REV_16_13_J

marker create marker_name = .MODEL_2.PART13.MK_REV_13_14_I &
    location = -16.5934352886, 36.5, 2.73440183281 &
    orientation = 155.521781357, 125.414247641, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART14.MK_REV_13_14_J &
    location = -16.5934352886, 36.5, 2.73440183281 &
    orientation = 155.521781357, 125.414247641, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_13_14 &
    i_marker_name = .MODEL_2.PART13.MK_REV_13_14_I &
    j_marker_name = .MODEL_2.PART14.MK_REV_13_14_J

marker create marker_name = .MODEL_2.PART14.MK_REV_14_11_I &
    location = -16.0602429764, 36.5, 0.806785180507 &
    orientation = 163.147045261, 102.4407564, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART11.MK_REV_14_11_J &
    location = -16.0602429764, 36.5, 0.806785180507 &
    orientation = 163.147045261, 102.4407564, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_14_11 &
    i_marker_name = .MODEL_2.PART14.MK_REV_14_11_I &
    j_marker_name = .MODEL_2.PART11.MK_REV_14_11_J

marker create marker_name = .MODEL_2.PART11.MK_REV_11_12_I &
    location = -14.0602429764, 36.5, 0.806785180507 &
    orientation = 138.835007812, 99.8446109063, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART12.MK_REV_11_12_J &
    location = -14.0602429764, 36.5, 0.806785180507 &
    orientation = 138.835007812, 99.8446109063, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_11_12 &
    i_marker_name = .MODEL_2.PART11.MK_REV_11_12_I &
    j_marker_name = .MODEL_2.PART12.MK_REV_11_12_J

marker create marker_name = .MODEL_2.PART12.MK_REV_12_9_I &
    location = -12.3201882704, 36.5, -0.17922172624 &
    orientation = 159.333692673, 87.2569893492, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART9.MK_REV_12_9_J &
    location = -12.3201882704, 36.5, -0.17922172624 &
    orientation = 159.333692673, 87.2569893492, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_12_9 &
    i_marker_name = .MODEL_2.PART12.MK_REV_12_9_I &
    j_marker_name = .MODEL_2.PART9.MK_REV_12_9_J

marker create marker_name = .MODEL_2.PART9.MK_REV_9_10_I &
    location = -10.9059747081, 36.5, 1.23499183613 &
    orientation = 142.000994786, 70.2640269507, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART10.MK_REV_9_10_J &
    location = -10.9059747081, 36.5, 1.23499183613 &
    orientation = 142.000994786, 70.2640269507, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_9_10 &
    i_marker_name = .MODEL_2.PART9.MK_REV_9_10_I &
    j_marker_name = .MODEL_2.PART10.MK_REV_9_10_J

marker create marker_name = .MODEL_2.PART10.MK_REV_10_7_I &
    location = -8.97835805576, 36.5, 1.76818414832 &
    orientation = 167.019483046, 73.5541516013, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART7.MK_REV_10_7_J &
    location = -8.97835805576, 36.5, 1.76818414832 &
    orientation = 167.019483046, 73.5541516013, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_10_7 &
    i_marker_name = .MODEL_2.PART10.MK_REV_10_7_I &
    j_marker_name = .MODEL_2.PART7.MK_REV_10_7_J

marker create marker_name = .MODEL_2.PART7.MK_REV_7_8_I &
    location = -8.97835805576, 36.5, 3.76818414832 &
    orientation = 167.019483046, 49.5685943013, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART8.MK_REV_7_8_J &
    location = -8.97835805576, 36.5, 3.76818414832 &
    orientation = 167.019483046, 49.5685943013, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_7_8 &
    i_marker_name = .MODEL_2.PART7.MK_REV_7_8_I &
    j_marker_name = .MODEL_2.PART8.MK_REV_7_8_J

marker create marker_name = .MODEL_2.PART8.MK_REV_8_5_I &
    location = -7.99235114901, 36.5, 5.50823885431 &
    orientation = -177.068660701, 69.3584527421, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART5.MK_REV_8_5_J &
    location = -7.99235114901, 36.5, 5.50823885431 &
    orientation = -177.068660701, 69.3584527421, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_8_5 &
    i_marker_name = .MODEL_2.PART8.MK_REV_8_5_I &
    j_marker_name = .MODEL_2.PART5.MK_REV_8_5_J

marker create marker_name = .MODEL_2.PART5.MK_REV_5_6_I &
    location = -9.40656471139, 36.5, 6.92245241668 &
    orientation = -155.521781357, 54.5857523592, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART6.MK_REV_5_6_J &
    location = -9.40656471139, 36.5, 6.92245241668 &
    orientation = -155.521781357, 54.5857523592, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_5_6 &
    i_marker_name = .MODEL_2.PART5.MK_REV_5_6_I &
    j_marker_name = .MODEL_2.PART6.MK_REV_5_6_J

marker create marker_name = .MODEL_2.PART6.MK_REV_6_3_I &
    location = -9.93975702357, 36.5, 8.85006906898 &
    orientation = -163.147045261, 77.5592435997, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART3.MK_REV_6_3_J &
    location = -9.93975702357, 36.5, 8.85006906898 &
    orientation = -163.147045261, 77.5592435997, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_6_3 &
    i_marker_name = .MODEL_2.PART6.MK_REV_6_3_I &
    j_marker_name = .MODEL_2.PART3.MK_REV_6_3_J

marker create marker_name = .MODEL_2.PART3.MK_REV_3_4_I &
    location = -11.9397570236, 36.5, 8.85006906899 &
    orientation = -138.835007812, 80.1553890937, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART4.MK_REV_3_4_J &
    location = -11.9397570236, 36.5, 8.85006906899 &
    orientation = -138.835007812, 80.1553890937, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_3_4 &
    i_marker_name = .MODEL_2.PART3.MK_REV_3_4_I &
    j_marker_name = .MODEL_2.PART4.MK_REV_3_4_J

marker create marker_name = .MODEL_2.PART4.MK_REV_4_17_I &
    location = -13.6798117296, 36.5, 9.83607597573 &
    orientation = -159.333692673, 92.7430106508, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART17.MK_REV_4_17_J &
    location = -13.6798117296, 36.5, 9.83607597573 &
    orientation = -159.333692673, 92.7430106508, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint revolute joint_name = .MODEL_2.AUTO_REV_4_17 &
    i_marker_name = .MODEL_2.PART4.MK_REV_4_17_I &
    j_marker_name = .MODEL_2.PART17.MK_REV_4_17_J

marker create marker_name = .MODEL_2.PART2.MK_FIX_CAP_PANEL_I &
    location = -10.6195687531, 33, 9.02929079522 &
    orientation = 90, 119.538181751, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.PART4.MK_FIX_CAP_PANEL_J &
    location = -10.6195687531, 33, 9.02929079522 &
    orientation = 90, 119.538181751, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint fixed joint_name = .MODEL_2.AUTO_FIX_CAP_PANEL4 &
    i_marker_name = .MODEL_2.PART2.MK_FIX_CAP_PANEL_I &
    j_marker_name = .MODEL_2.PART4.MK_FIX_CAP_PANEL_J

marker create marker_name = .MODEL_2.PART2.MK_FIX_CAP_GROUND_I &
    location = -10.6195687531, 33, 9.02929079522 &
    orientation = 90, 119.538181751, 0 &
    relative_to = .MODEL_2.GROUND

marker create marker_name = .MODEL_2.GROUND.MK_FIX_CAP_GROUND_J &
    location = -10.6195687531, 33, 9.02929079522 &
    orientation = 90, 119.538181751, 0 &
    relative_to = .MODEL_2.GROUND

constraint create joint fixed joint_name = .MODEL_2.AUTO_FIX_CAP_GROUND &
    i_marker_name = .MODEL_2.PART2.MK_FIX_CAP_GROUND_I &
    j_marker_name = .MODEL_2.GROUND.MK_FIX_CAP_GROUND_J
