import { Button, Form, Input, Row } from "antd"
import { useForm } from "antd/es/form/Form"
import React, { useEffect, useMemo, useState } from "react"
import { initialValues } from "./EducationalInstitutionList"
import { ResourceFilterType } from "@/app/types/Resource"
import useQueryApiClient from "@/app/utils/useQueryApiClient"
import SearchSelectInput from "@/app/components/searchSelectInput"
import { ClassifierResponse } from "@/app/types/Classifiers"
import { EducationalInstitution as EducationalInstitutionType, EducationalInstitutionFilter } from "@/app/types/EducationalInstitution"
import { SelectOption } from "@/app/types/Antd"
import { useSession } from "next-auth/react"

type EducationalInstitutionFiltersProps = {
  activeFilters: EducationalInstitutionFilter,
  filterState: (val: EducationalInstitutionFilter) => void
  refresh: (val: EducationalInstitutionFilter) => void,
  defaultEducationalInstitutionStatusIds: string | null,
  defaultSupervisorIds: string | null
}

const EducationalInstitutionFilters = ({activeFilters, filterState, refresh, defaultEducationalInstitutionStatusIds, defaultSupervisorIds}: EducationalInstitutionFiltersProps) => {
    const [educationalInstitutionOptions, setEducationalInstitutionOptions] = useState<SelectOption[]>([])
    const [supervisorOptions, setSupervisorOptions] = useState<SelectOption[]>([])
    const [statusOptions, setStatusOptions] = useState<SelectOption[]>([])

    const {data: sessionData} = useSession()

    const [form] = useForm()

    const {} = useQueryApiClient({
      request: {
        url: '/educationalInstitutions',
      },
      onSuccess: (response: EducationalInstitutionType[]) => {
        setEducationalInstitutionOptions(response.map((edu: EducationalInstitutionType) => ({
          value: edu.id,
          label: edu.name
        })))

        const uniqueSupervisors = new Set();
        const supervisorOptions: SelectOption[] = []
        
        response.forEach(edu => {
            if (!uniqueSupervisors.has(edu.supervisor.id)) {
                uniqueSupervisors.add(edu.supervisor.id);

                supervisorOptions.push({
                    value: edu.supervisor.id,
                    label: edu.supervisor.name,
                    rest: edu.supervisor.code
                })
            }
        });

        setSupervisorOptions(supervisorOptions)
      }
    });

    const {} = useQueryApiClient({
      request: {
        url: `/classifiers`,
        data: {
          type: 'educational_institution_status',
          includeDisabled: false
        }
      },
      onSuccess: (response: ClassifierResponse[]) => {
        setStatusOptions(response.map(status => ({
          value: status.id,
          label: status.value,
        })))
      }
    })

    const filterList = [
      {
        name: 'Izglītības iestāde',
        key: 'educationalInstitution',
        render: () => ( <Form.Item name="educationalInstitution" label="Izglītības iestāde">
          <SearchSelectInput options={educationalInstitutionOptions} mode="multiple" disabled={sessionData?.user?.permissionType === 'EducationalInstitution'}/>
        </Form.Item>)
      },
      {
        name: 'Vadošā iestāde',
        key: 'supervisorIds',
        render: () => ( <Form.Item name="supervisorIds" label="Vadošā iestāde">
          <SearchSelectInput options={supervisorOptions} mode="multiple" disabled={sessionData?.user?.permissionType === 'Supervisor'}/>
        </Form.Item>)
      },
      {
        name: 'Statuss',
        key: 'educationalInstitutionStatusIds',
        render: () => ( <Form.Item name="educationalInstitutionStatusIds" label="Statuss">
          <SearchSelectInput options={statusOptions}/>
        </Form.Item>)
      },
    ]

    const onReset = () => {
        form.resetFields()
    
        filterState({
          ...initialValues,
          take: activeFilters.take,
        })
        refresh({
          ...initialValues,
          take: activeFilters.take,
        })
    }

    const onFinish = (values: any) => {
      const filters: EducationalInstitutionFilter = {
        ...activeFilters,
        ...initialValues,
        educationalInstitutionIds: values.educationalInstitution,
        supervisorIds: values.supervisorIds,
        educationalInstitutionStatusIds: values.educationalInstitutionStatusIds,
      }

      filterState(filters)
      refresh(filters)
    }

    const initialData = useMemo(() => {
      const supervisorIds = sessionData?.user?.permissionType === 'Supervisor'
      ? [sessionData?.user?.supervisor]
      : defaultSupervisorIds ? [+defaultSupervisorIds] : []
      return {
          supervisorIds: supervisorIds,
          educationalInstitutionStatusIds: defaultEducationalInstitutionStatusIds ?? [],
          educationalInstitution: sessionData?.user?.permissionType === 'EducationalInstitution' ? [sessionData?.user?.educationalInstitutionId] : []
      }
    }, [])

    return (
        <Form
            form={form}
            name="EducationalInstitutionListFilters"
            onFinish={onFinish}
            layout="vertical"
            initialValues={initialData}
        >
            <div className="grid grid-cols-2 md:grid-cols-4 xl:grid-cols-6 gap-4">
              {filterList.map(filter => (
                <React.Fragment key={filter.key}>
                  {filter.render()}
                </React.Fragment>
              ))}
            </div>
            <Form.Item>
            <Row>
                <div className='flex gap-2'>
                    <Button type="primary" htmlType="submit">
                        Atlasīt
                    </Button>
                    <Button htmlType="button" onClick={onReset}>
                        Atcelt
                    </Button>
                </div>
            </Row>
        </Form.Item>
      </Form>
    )
}

export default EducationalInstitutionFilters