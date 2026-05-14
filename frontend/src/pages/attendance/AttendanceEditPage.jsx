import { EntityFormPage } from '../../components/entity/CrudPages';
import { attendanceConfig } from '../../config/entities';

export function AttendanceEditPage() {
  return <EntityFormPage config={attendanceConfig} mode="edit" />;
}