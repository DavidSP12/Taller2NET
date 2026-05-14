import { EntityListPage } from '../../components/entity/CrudPages';
import { courseConfig } from '../../config/entities';

export function CoursesListPage() {
  return <EntityListPage config={courseConfig} />;
}