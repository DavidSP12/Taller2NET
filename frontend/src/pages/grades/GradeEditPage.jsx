import { EntityFormPage } from '../../components/entity/CrudPages';
import { gradeConfig } from '../../config/entities';

export function GradeEditPage() {
  return <EntityFormPage config={gradeConfig} mode="edit" />;
}