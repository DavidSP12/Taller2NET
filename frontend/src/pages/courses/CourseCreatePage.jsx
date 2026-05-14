import { EntityFormPage } from '../../components/entity/CrudPages';
import { courseConfig } from '../../config/entities';

export function CourseCreatePage() {
  return <EntityFormPage config={courseConfig} mode="create" />;
}