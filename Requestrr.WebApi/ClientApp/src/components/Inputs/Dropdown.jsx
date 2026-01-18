
import {useMemo, useState} from "react";
import { Select } from "react-dropdown-select";

import {
  FormGroup,
} from "reactstrap";


function Dropdown(props) {
  const [selectedValues, setSelectedValues] = useState([{ name: "", value: 838738 }]);


  let newSelectedValues = props.items.length > 0 ? props.items.filter(x => x.value === props.value).length > 0 ? props.items.filter(x => x.value === props.value) : [props.items[0]] : [];
  if (JSON.stringify(selectedValues) !== JSON.stringify(newSelectedValues))
    setSelectedValues(newSelectedValues);

  const disabled = useMemo(() => props.disabled, [props.disabled]);

  const onValueChange = (value) =>
    props.onChange(value);


  return (
    <FormGroup className={props.className}>
      <label
        className="form-control-label">
        {props.name}
      </label>

      <Select
        placeholder=""
        disabled={disabled}
        className="dropdown"
        options={props.items}
        values={selectedValues}
        labelField="name"
        valueField="value"
        searchable={false}
        clearable={false}
        onChange={(value) => {
          if (typeof (value) !== "undefined" && value.length > 0) {
            onValueChange(value[0].value);
          }
        }}
      />
    </FormGroup>
  );
}

export default Dropdown;