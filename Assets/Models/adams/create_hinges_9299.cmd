! Generated from 929.stp by extract_adams_hinges.py.
! Import into a CLEAN .MODEL_9299 after importing the STEP geometry.
! Existing manually created joints must be removed first, otherwise they
! will duplicate these constraints and produce an overconstrained model.
! Marker orientations use Adams body-fixed 3-1-3 Euler angles in degrees.

marker create marker_name = .MODEL_9299.PART16.MK_16_17_I &
    location = -15.0940252919, 36.5, 8.42186241336 &
    orientation = -142.000994786, 109.735973049, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART17.MK_16_17_J &
    location = -15.0940252919, 36.5, 8.42186241336 &
    orientation = -142.000994786, 109.735973049, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_16_17 &
    i_marker_name = .MODEL_9299.PART16.MK_16_17_I &
    j_marker_name = .MODEL_9299.PART17.MK_16_17_J

marker create marker_name = .MODEL_9299.PART17.MK_17_14_I &
    location = -17.0216419442, 36.5, 7.88867010117 &
    orientation = -167.019483046, 106.445848399, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART14.MK_17_14_J &
    location = -17.0216419442, 36.5, 7.88867010117 &
    orientation = -167.019483046, 106.445848399, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_17_14 &
    i_marker_name = .MODEL_9299.PART17.MK_17_14_I &
    j_marker_name = .MODEL_9299.PART14.MK_17_14_J

marker create marker_name = .MODEL_9299.PART14.MK_14_15_I &
    location = -17.0216419442, 36.5, 5.88867010117 &
    orientation = -167.019483046, 130.431405699, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART15.MK_14_15_J &
    location = -17.0216419442, 36.5, 5.88867010117 &
    orientation = -167.019483046, 130.431405699, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_14_15 &
    i_marker_name = .MODEL_9299.PART14.MK_14_15_I &
    j_marker_name = .MODEL_9299.PART15.MK_14_15_J

marker create marker_name = .MODEL_9299.PART15.MK_15_12_I &
    location = -18.007648851, 36.5, 4.14861539518 &
    orientation = 177.068660701, 110.641547258, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART12.MK_15_12_J &
    location = -18.007648851, 36.5, 4.14861539518 &
    orientation = 177.068660701, 110.641547258, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_15_12 &
    i_marker_name = .MODEL_9299.PART15.MK_15_12_I &
    j_marker_name = .MODEL_9299.PART12.MK_15_12_J

marker create marker_name = .MODEL_9299.PART12.MK_12_13_I &
    location = -16.5934352886, 36.5, 2.73440183281 &
    orientation = 155.521781357, 125.414247641, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART13.MK_12_13_J &
    location = -16.5934352886, 36.5, 2.73440183281 &
    orientation = 155.521781357, 125.414247641, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_12_13 &
    i_marker_name = .MODEL_9299.PART12.MK_12_13_I &
    j_marker_name = .MODEL_9299.PART13.MK_12_13_J

marker create marker_name = .MODEL_9299.PART13.MK_13_10_I &
    location = -16.0602429764, 36.5, 0.806785180507 &
    orientation = 163.147045261, 102.4407564, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART10.MK_13_10_J &
    location = -16.0602429764, 36.5, 0.806785180507 &
    orientation = 163.147045261, 102.4407564, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_13_10 &
    i_marker_name = .MODEL_9299.PART13.MK_13_10_I &
    j_marker_name = .MODEL_9299.PART10.MK_13_10_J

marker create marker_name = .MODEL_9299.PART10.MK_10_11_I &
    location = -14.0602429764, 36.5, 0.806785180507 &
    orientation = 138.835007812, 99.8446109063, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART11.MK_10_11_J &
    location = -14.0602429764, 36.5, 0.806785180507 &
    orientation = 138.835007812, 99.8446109063, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_10_11 &
    i_marker_name = .MODEL_9299.PART10.MK_10_11_I &
    j_marker_name = .MODEL_9299.PART11.MK_10_11_J

marker create marker_name = .MODEL_9299.PART11.MK_11_8_I &
    location = -12.3201882704, 36.5, -0.17922172624 &
    orientation = 159.333692673, 87.2569893492, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART8.MK_11_8_J &
    location = -12.3201882704, 36.5, -0.17922172624 &
    orientation = 159.333692673, 87.2569893492, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_11_8 &
    i_marker_name = .MODEL_9299.PART11.MK_11_8_I &
    j_marker_name = .MODEL_9299.PART8.MK_11_8_J

marker create marker_name = .MODEL_9299.PART8.MK_8_9_I &
    location = -10.9059747081, 36.5, 1.23499183613 &
    orientation = 142.000994786, 70.2640269507, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART9.MK_8_9_J &
    location = -10.9059747081, 36.5, 1.23499183613 &
    orientation = 142.000994786, 70.2640269507, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_8_9 &
    i_marker_name = .MODEL_9299.PART8.MK_8_9_I &
    j_marker_name = .MODEL_9299.PART9.MK_8_9_J

marker create marker_name = .MODEL_9299.PART9.MK_9_6_I &
    location = -8.97835805576, 36.5, 1.76818414832 &
    orientation = 167.019483046, 73.5541516013, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART6.MK_9_6_J &
    location = -8.97835805576, 36.5, 1.76818414832 &
    orientation = 167.019483046, 73.5541516013, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_9_6 &
    i_marker_name = .MODEL_9299.PART9.MK_9_6_I &
    j_marker_name = .MODEL_9299.PART6.MK_9_6_J

marker create marker_name = .MODEL_9299.PART6.MK_6_7_I &
    location = -8.97835805576, 36.5, 3.76818414832 &
    orientation = 167.019483046, 49.5685943013, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART7.MK_6_7_J &
    location = -8.97835805576, 36.5, 3.76818414832 &
    orientation = 167.019483046, 49.5685943013, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_6_7 &
    i_marker_name = .MODEL_9299.PART6.MK_6_7_I &
    j_marker_name = .MODEL_9299.PART7.MK_6_7_J

marker create marker_name = .MODEL_9299.PART7.MK_7_4_I &
    location = -7.99235114901, 36.5, 5.50823885431 &
    orientation = -177.068660701, 69.3584527421, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART4.MK_7_4_J &
    location = -7.99235114901, 36.5, 5.50823885431 &
    orientation = -177.068660701, 69.3584527421, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_7_4 &
    i_marker_name = .MODEL_9299.PART7.MK_7_4_I &
    j_marker_name = .MODEL_9299.PART4.MK_7_4_J

marker create marker_name = .MODEL_9299.PART4.MK_4_5_I &
    location = -9.40656471139, 36.5, 6.92245241668 &
    orientation = -155.521781357, 54.5857523592, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART5.MK_4_5_J &
    location = -9.40656471139, 36.5, 6.92245241668 &
    orientation = -155.521781357, 54.5857523592, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_4_5 &
    i_marker_name = .MODEL_9299.PART4.MK_4_5_I &
    j_marker_name = .MODEL_9299.PART5.MK_4_5_J

marker create marker_name = .MODEL_9299.PART5.MK_5_2_I &
    location = -9.93975702357, 36.5, 8.85006906898 &
    orientation = -163.147045261, 77.5592435997, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART2.MK_5_2_J &
    location = -9.93975702357, 36.5, 8.85006906898 &
    orientation = -163.147045261, 77.5592435997, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_5_2 &
    i_marker_name = .MODEL_9299.PART5.MK_5_2_I &
    j_marker_name = .MODEL_9299.PART2.MK_5_2_J

marker create marker_name = .MODEL_9299.PART2.MK_2_3_I &
    location = -11.9397570236, 36.5, 8.85006906899 &
    orientation = -138.835007812, 80.1553890937, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART3.MK_2_3_J &
    location = -11.9397570236, 36.5, 8.85006906899 &
    orientation = -138.835007812, 80.1553890937, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_2_3 &
    i_marker_name = .MODEL_9299.PART2.MK_2_3_I &
    j_marker_name = .MODEL_9299.PART3.MK_2_3_J

marker create marker_name = .MODEL_9299.PART3.MK_3_16_I &
    location = -13.6798117296, 36.5, 9.83607597573 &
    orientation = -159.333692673, 92.7430106508, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.PART16.MK_3_16_J &
    location = -13.6798117296, 36.5, 9.83607597573 &
    orientation = -159.333692673, 92.7430106508, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint revolute joint_name = .MODEL_9299.AUTO_REV_3_16 &
    i_marker_name = .MODEL_9299.PART3.MK_3_16_I &
    j_marker_name = .MODEL_9299.PART16.MK_3_16_J

marker create marker_name = .MODEL_9299.PART16.MK_FIX_16_I &
    location = -15.0940252919, 36.5, 8.42186241336 &
    orientation = -142.000994786, 109.735973049, 0 &
    relative_to = .MODEL_9299.GROUND

marker create marker_name = .MODEL_9299.GROUND.MK_FIX_16_J &
    location = -15.0940252919, 36.5, 8.42186241336 &
    orientation = -142.000994786, 109.735973049, 0 &
    relative_to = .MODEL_9299.GROUND

constraint create joint fixed joint_name = .MODEL_9299.AUTO_FIX_16 &
    i_marker_name = .MODEL_9299.PART16.MK_FIX_16_I &
    j_marker_name = .MODEL_9299.GROUND.MK_FIX_16_J
