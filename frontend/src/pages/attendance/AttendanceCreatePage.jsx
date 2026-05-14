import { EntityFormPage } from '../../components/entity/CrudPages';
import { attendanceConfig } from '../../config/entities';

export function AttendanceCreatePage() {
  return <EntityFormPage config={attendanceConfig} mode="create" />;
}