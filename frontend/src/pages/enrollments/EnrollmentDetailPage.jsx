import { EntityDetailPage } from '../../components/entity/CrudPages';
import { enrollmentConfig } from '../../config/entities';

export function EnrollmentDetailPage() {
  return <EntityDetailPage config={enrollmentConfig} />;
}