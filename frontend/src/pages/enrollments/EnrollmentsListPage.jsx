import { EntityListPage } from '../../components/entity/CrudPages';
import { enrollmentConfig } from '../../config/entities';

export function EnrollmentsListPage() {
  return <EntityListPage config={enrollmentConfig} />;
}