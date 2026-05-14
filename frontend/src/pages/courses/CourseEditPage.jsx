import { EntityFormPage } from '../../components/entity/CrudPages';
import { courseConfig } from '../../config/entities';

export function CourseEditPage() {
  return <EntityFormPage config={courseConfig} mode="edit" />;
}