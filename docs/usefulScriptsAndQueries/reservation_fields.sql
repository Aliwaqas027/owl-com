-- find reservation fields for warehouses for company
SELECT warehouses."Name", reservation_fields."Id", reservation_fields."DerivedFromFieldId", reservation_fields."CompanyId", reservation_fields."WarehouseId", reservation_fields."DoorId", reservation_fields."Type", reservation_field_names."name" FROM reservation_fields
LEFT JOIN reservation_field_names 
ON reservation_field_names."fieldId" = reservation_fields."Id"
LEFT JOIN warehouses
ON warehouses."Id" = reservation_fields."WarehouseId"
WHERE warehouses."CompanyId" = 4
ORDER BY reservation_fields."Id" ASC;

-- find reservation fields for doors for company
SELECT doors."Name", warehouses."Name", reservation_fields."Id", reservation_fields."DerivedFromFieldId", reservation_fields."CompanyId", reservation_fields."WarehouseId", reservation_fields."DoorId", reservation_fields."Type", reservation_field_names."name" FROM reservation_fields
LEFT JOIN reservation_field_names 
ON reservation_field_names."fieldId" = reservation_fields."Id"
LEFT JOIN doors
ON doors."Id" = reservation_fields."DoorId"
LEFT JOIN warehouses
ON warehouses."Id" = doors."WarehouseId"
WHERE warehouses."CompanyId" = 4
ORDER BY reservation_fields."Id" ASC;

-- update res field derived id
UPDATE reservation_fields 
SET "DerivedFromFieldId" = 110
WHERE "Id" = 194; 

-- check reservation fields for matches
SELECT originals."Id", originalsnames."name", deriveds."Id", derivedsnames."name" FROM reservation_fields as originals
LEFT JOIN reservation_fields as deriveds
ON originals."DerivedFromFieldId" = deriveds."Id"
LEFT JOIN reservation_field_names as originalsnames
ON originals."Id" = originalsnames."fieldId"
LEFT JOIN reservation_field_names as derivedsnames
ON deriveds."Id" = derivedsnames."fieldId";
