! MODEL_2: keep alternating eight motions from the 16-motion test.
! This removes the other eight FOLD_ALL motions.  The eight retained motions
! all remain 0 -> +3 degrees from 0 to 5 seconds.
! The mechanism will retain 2 DOF; it is a symmetry test, not a unique final pose.

constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_18_15
constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_16_13
constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_14_11
constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_12_9
constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_10_7
constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_8_5
constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_6_3
constraint delete motion_generator motion_name = .MODEL_2.FOLD_ALL_4_17
