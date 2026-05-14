import { EntityDetailPage } from '../../components/entity/CrudPages';
import { courseConfig } from '../../config/entities';

export function CourseDetailPage() {
  return <EntityDetailPage config={courseConfig} />;
}