! MODEL_2 uniform rigid-body material assignment.
! Model length unit: mm; density 1.0E-6 kg/mm^3 = 1000 kg/m^3.
! Young's modulus and Poisson's ratio are required by Adams to create a material.

material create material_name = .MODEL_2.UNIFORM_MATERIAL &
    youngs_modulus = 1000.0 &
    poissons_ratio = 0.30 &
    density = 1.0E-6

part modify rigid_body mass_properties part_name = .MODEL_2.PART2 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART3 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART4 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART5 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART6 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART7 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART8 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART9 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART10 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART11 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART12 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART13 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART14 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART15 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART16 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART17 &
    material = .MODEL_2.UNIFORM_MATERIAL
part modify rigid_body mass_properties part_name = .MODEL_2.PART18 &
    material = .MODEL_2.UNIFORM_MATERIAL
